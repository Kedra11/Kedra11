#!/usr/bin/env python3
"""
Priority scraper — заходит в веб-интерфейс Priority у ТЕБЯ на машине
(там, где есть сеть до внутреннего хоста) и выгружает данные.

Два режима:

  1) python scrape.py setup
     Открывает браузер, логинится (или ты логинишься руками) и сохраняет
     сессию в auth_state.json. Делается ОДИН раз — потом логин не нужен.

  2) python scrape.py run
     Переиспользует сохранённую сессию, открывает нужный экран и
     выгружает данные в output/ (CSV + Excel).

Секреты берутся из .env (см. .env.example). В код ничего не зашивается.
"""

import os
import re
import sys
import argparse
from pathlib import Path

from dotenv import load_dotenv
from playwright.sync_api import sync_playwright, TimeoutError as PWTimeout

# --- конфигурация ---------------------------------------------------------

BASE_DIR = Path(__file__).resolve().parent
AUTH_FILE = BASE_DIR / "auth_state.json"      # сюда сохраняется залогиненная сессия
OUTPUT_DIR = BASE_DIR / "output"

load_dotenv(BASE_DIR / ".env")

URL = os.getenv("PRIORITY_URL", "").strip()
USER = os.getenv("PRIORITY_USER", "").strip()
PASSWORD = os.getenv("PRIORITY_PASS", "").strip()
HEADLESS = os.getenv("HEADLESS", "0").strip() in ("1", "true", "True", "yes")
# "msedge" — рулить твоим установленным Edge; пусто — свой Chromium от Playwright
CHANNEL = os.getenv("PW_CHANNEL", "").strip()

# Вкладки нижней панели заявки (имена кнопок — как в записи codegen)
DETAIL_TABS = ["תאור התקלה", "תאור התיקון", "עבודה", "חלקים", "דו שיח פנימי"]

# Для массового прохода (режим batch) — 4 вкладки, которые нужны пользователю
BATCH_TABS = ["תאור התקלה", "תאור התיקון", "עבודה", "חלקים"]
BATCH_LIMIT = int(os.getenv("BATCH_LIMIT", "5") or "5")   # сколько заявок за прогон
SC_FROM = os.getenv("SC_FROM", "SC26000193").strip()
SC_TO = os.getenv("SC_TO", "").strip()                    # напр. SC22000001; пусто = не проверять

# Кандидаты селекторов для формы логина. Priority у всех чуть разный,
# поэтому пробуем несколько вариантов; если не сработает — залогинишься руками.
USER_SELECTORS = [
    'input[name="username"]', 'input[name="user"]', 'input[type="text"]',
    'input[id*="user" i]', 'input[placeholder*="польз" i]',
    'input[placeholder*="user" i]', 'input[placeholder*="login" i]',
]
PASS_SELECTORS = [
    'input[name="password"]', 'input[type="password"]',
    'input[id*="pass" i]', 'input[placeholder*="пароль" i]',
    'input[placeholder*="pass" i]',
]
SUBMIT_SELECTORS = [
    'button[type="submit"]', 'input[type="submit"]',
    'button:has-text("Вход")', 'button:has-text("Войти")',
    'button:has-text("Login")', 'button:has-text("Sign in")',
]


def _launch(pw, headless):
    """Запуск браузера. Если PW_CHANNEL=msedge — берём установленный Edge."""
    kwargs = {"headless": headless}
    if CHANNEL:
        kwargs["channel"] = CHANNEL
    return pw.chromium.launch(**kwargs)


def _require_url():
    if not URL or "your-priority-host" in URL:
        sys.exit("❌ Впиши свой PRIORITY_URL в файл .env (см. .env.example).")


def _first_visible(page, selectors):
    """Вернуть первый видимый элемент из списка селекторов, или None."""
    for sel in selectors:
        loc = page.locator(sel).first
        try:
            if loc.count() > 0 and loc.is_visible():
                return loc
        except Exception:
            continue
    return None


# --- режим setup: логин и сохранение сессии --------------------------------

def cmd_setup():
    _require_url()
    with sync_playwright() as pw:
        # для первичной настройки браузер всегда видимый — так удобнее
        browser = _launch(pw, headless=False)
        context = browser.new_context(accept_downloads=True)
        page = context.new_page()

        print(f"→ Открываю {URL}")
        page.goto(URL, wait_until="domcontentloaded")

        logged_in = False
        if USER and PASSWORD:
            print("→ Пробую залогиниться автоматически…")
            u = _first_visible(page, USER_SELECTORS)
            p = _first_visible(page, PASS_SELECTORS)
            if u and p:
                u.fill(USER)
                p.fill(PASSWORD)
                btn = _first_visible(page, SUBMIT_SELECTORS)
                if btn:
                    btn.click()
                else:
                    p.press("Enter")
                try:
                    page.wait_for_load_state("networkidle", timeout=15000)
                    logged_in = True
                    print("✓ Похоже, залогинился автоматически.")
                except PWTimeout:
                    print("… авто-логин не подтвердился, дологинься руками.")
            else:
                print("… не нашёл поля логина автоматически — залогинься руками.")

        if not logged_in:
            print("\n" + "=" * 60)
            print("ЗАЛОГИНЬСЯ В ОТКРЫВШЕМСЯ БРАУЗЕРЕ РУКАМИ.")
            print("Когда увидишь главный экран Priority — вернись сюда")
            print("и нажми Enter, чтобы сохранить сессию.")
            print("=" * 60)
            input("\n[Enter] когда залогинился… ")

        context.storage_state(path=str(AUTH_FILE))
        print(f"✓ Сессия сохранена в {AUTH_FILE.name}. Теперь запускай: python scrape.py run")
        browser.close()


# --- режим run: выгрузка данных -------------------------------------------

def cmd_run():
    _require_url()
    if not AUTH_FILE.exists():
        sys.exit("❌ Нет сохранённой сессии. Сначала: python scrape.py setup")

    OUTPUT_DIR.mkdir(exist_ok=True)

    with sync_playwright() as pw:
        browser = _launch(pw, headless=HEADLESS)
        context = browser.new_context(
            storage_state=str(AUTH_FILE), accept_downloads=True
        )
        page = context.new_page()

        print(f"→ Открываю {URL}")
        page.goto(URL, wait_until="domcontentloaded")
        page.wait_for_load_state("networkidle")

        # ==================================================================
        # ТУТ — навигация к нужному экрану/отчёту и выгрузка.
        # Пока стоит заглушка: дампим ВСЕ HTML-таблицы, что есть на странице.
        # Как только скажешь, ЧТО именно тебе нужно (какой экран/отчёт),
        # я впишу сюда точные клики. Проще всего снять их через codegen:
        #     playwright codegen <твой PRIORITY_URL>
        # (кликаешь руками — Playwright пишет готовый код).
        #
        # ВАЖНО: в Priority часто есть встроенная кнопка "Экспорт в Excel".
        # Кликнуть её и поймать скачивание — самый надёжный путь, см.
        # helper download_via_click() ниже.
        # ==================================================================

        dump_all_tables(page, prefix="page")

        browser.close()
        print(f"\n✓ Готово. Файлы в папке: {OUTPUT_DIR}")


# --- helpers для выгрузки --------------------------------------------------

def dump_all_tables(page, prefix="table"):
    """Найти все HTML-таблицы и сохранить каждую в CSV + один общий Excel."""
    import pandas as pd

    html = page.content()
    try:
        tables = pd.read_html(html)  # требует lxml/html5lib — есть в pandas
    except ValueError:
        print("… стандартных <table> на странице не нашлось.")
        print("  (Priority мог отрисовать грид не как <table> — тогда лучше")
        print("   встроенный экспорт в Excel, см. download_via_click().)")
        return

    if not tables:
        print("… таблиц не найдено.")
        return

    xlsx_path = OUTPUT_DIR / f"{prefix}.xlsx"
    with pd.ExcelWriter(xlsx_path) as writer:
        for i, df in enumerate(tables, 1):
            csv_path = OUTPUT_DIR / f"{prefix}_{i}.csv"
            df.to_csv(csv_path, index=False, encoding="utf-8-sig")
            df.to_excel(writer, sheet_name=f"table_{i}", index=False)
            print(f"  ✓ таблица {i}: {len(df)} строк → {csv_path.name}")
    print(f"  ✓ всё вместе → {xlsx_path.name}")


def download_via_click(page, click_selector, save_name):
    """
    Нажать кнопку/ссылку, которая инициирует скачивание файла
    (например встроенный 'Экспорт в Excel' в Priority), и сохранить файл.
    """
    with page.expect_download() as dl_info:
        page.click(click_selector)
    download = dl_info.value
    dest = OUTPUT_DIR / save_name
    download.save_as(str(dest))
    print(f"  ✓ скачано → {dest.name}")
    return dest


# --- режим discover: изучить структуру ОДНОЙ заявки ------------------------

def cmd_discover():
    """
    Открыть ОДНУ заявку и выгрузить содержимое всех вкладок «как есть»
    (текст + iframe'ы + таблицы + скриншот), чтобы увидеть структуру перед
    массовой выгрузкой. Ничего не 'помечать' руками не нужно — скрипт сам
    читает то, что видно на каждой вкладке.
    """
    _require_url()
    if not AUTH_FILE.exists():
        sys.exit("❌ Нет сессии. Сначала: python scrape.py setup")

    out = OUTPUT_DIR / "discover"
    out.mkdir(parents=True, exist_ok=True)

    with sync_playwright() as pw:
        browser = _launch(pw, headless=False)
        context = browser.new_context(
            storage_state=str(AUTH_FILE), accept_downloads=True
        )
        page = context.new_page()
        print(f"→ Открываю {URL}")
        page.goto(URL, wait_until="domcontentloaded")

        print("\n" + "=" * 60)
        print("В браузере ОТКРОЙ ОДНУ заявку так, чтобы снизу появились")
        print("вкладки (תאור התקלה, עבודה, חלקים …). Обычно:")
        print("  меню קריאות שרות → клик по строке заявки.")
        print("Потом вернись сюда и нажми Enter.")
        print("=" * 60)
        input("\n[Enter] когда заявка открыта… ")

        _dump_tab(page, out, "00_full")            # экран целиком — точка отсчёта
        for i, tab in enumerate(DETAIL_TABS, 1):
            try:
                page.get_by_role("button", name=tab).first.click()
                page.wait_for_timeout(1500)
                _dump_tab(page, out, f"{i:02d}_{_slug(tab)}", tab_label=tab)
                print(f"  ✓ вкладка «{tab}» выгружена")
            except Exception as e:
                print(f"  ⚠ вкладку «{tab}» не удалось открыть: {e}")

        print(f"\n✓ Готово. Смотри папку: {out}")
        print("  Пришли мне .txt (или скриншоты .png) оттуда — по ним соберу")
        print("  точную массовую выгрузку.")
        browser.close()


def _slug(s):
    return re.sub(r"[^\w]+", "_", s).strip("_") or "tab"


# --- режим batch: пройтись по заявкам и выгрузить вкладки, назвав по SC -----

def _read_sc(page):
    """Попытаться прочитать номер текущей заявки (SC########) для имени файла."""
    # 1) текущее выбранное поле
    for sel in [".priCurrentFieldStyle input", ".priCurrentFieldStyle"]:
        try:
            el = page.locator(sel).first
            if el.count():
                try:
                    v = el.input_value()
                except Exception:
                    v = el.inner_text()
                m = re.search(r"SC\d{6,8}", v or "")
                if m:
                    return m.group(0)
        except Exception:
            pass
    # 2) любое поле ввода на странице со значением SC…
    try:
        vals = page.eval_on_selector_all("input", "els => els.map(e => e.value || '')")
        for v in vals:
            m = re.search(r"SC\d{6,8}", v or "")
            if m:
                return m.group(0)
    except Exception:
        pass
    return None


def _next_record(page):
    """
    Перейти к следующей заявке так же, как это делает человек:
    ESC — выйти из вкладок обратно на экран выбора заявок,
    ArrowDown — перейти на строку ниже (следующий SC).
    """
    # ESC возвращает фокус на грид со списком заявок
    page.keyboard.press("Escape")
    page.wait_for_timeout(600)
    # на некоторых экранах нужен второй ESC — он безвреден, если уже на гриде
    page.keyboard.press("Escape")
    page.wait_for_timeout(600)
    # стрелка вниз по текущему полю грида — на следующую заявку
    try:
        page.locator(".priCurrentFieldStyle").first.press("ArrowDown")
    except Exception:
        page.keyboard.press("ArrowDown")
    page.wait_for_timeout(800)


def cmd_batch():
    """
    Пройтись по заявкам сверху вниз (SC26000193 → SC26000192 → …), для каждой
    выгрузить 4 вкладки (скриншот + текст), назвав файлы по номеру SC.
    Сначала запускается на BATCH_LIMIT заявках (по умолчанию 5) — как тест.
    """
    _require_url()
    if not AUTH_FILE.exists():
        sys.exit("❌ Нет сессии. Сначала: python scrape.py setup")

    out = OUTPUT_DIR / "batch"
    out.mkdir(parents=True, exist_ok=True)
    log_path = out / "_log.txt"
    logs = []

    def L(msg):
        print(msg)
        logs.append(msg)
        log_path.write_text("\n".join(logs), encoding="utf-8")

    with sync_playwright() as pw:
        browser = _launch(pw, headless=False)
        context = browser.new_context(
            storage_state=str(AUTH_FILE), accept_downloads=True
        )
        page = context.new_page()
        page.goto(URL, wait_until="domcontentloaded")

        print("\n" + "=" * 60)
        print("Открой экран קריאות שרות (список заявок) и КЛИКНИ по ПЕРВОЙ")
        print(f"заявке ({SC_FROM}) — так, чтобы снизу появились вкладки.")
        print("Потом вернись сюда и нажми Enter.")
        print("=" * 60)
        input("\n[Enter] когда первая заявка открыта… ")

        L(f"Старт. Лимит за прогон: {BATCH_LIMIT} заявок.")
        seen = set()
        for n in range(BATCH_LIMIT):
            sc = _read_sc(page) or f"UNKNOWN_{n + 1:04d}"
            L(f"[{n + 1}/{BATCH_LIMIT}] SC = {sc}")

            if sc in seen:
                L(f"  ⚠ SC повторился ({sc}) — переход на следующую заявку НЕ")
                L("     сработал. Останавливаюсь, чтобы не плодить дубли.")
                break
            seen.add(sc)

            for j, tab in enumerate(BATCH_TABS, 1):
                try:
                    page.get_by_role("button", name=tab).first.click()
                    page.wait_for_timeout(1200)
                    _dump_tab(page, out, f"{sc}_{j}_{_slug(tab)}", tab_label=tab)
                    L(f"    ✓ {tab}")
                except Exception as e:
                    L(f"    ⚠ {tab}: {e}")

            if SC_TO and sc == SC_TO:
                L(f"Достигнут SC_TO={SC_TO}. Стоп.")
                break

            _next_record(page)
            page.wait_for_timeout(1200)

        L(f"\n✓ Готово. Обработано уникальных заявок: {len(seen)}")
        L(f"  Папка: {out}")
        browser.close()


# маркеры служебного iframe редактора (панель шрифтов + внутренний JS) — их пропускаем
_EDITOR_NOISE = ("SpellCheckAddWord", "Comic Sans MS", "Wingdings")


def _dump_tab(page, out, name, tab_label=None):
    """Скриншот (JPEG) + очищенный текст страницы + текст из редакторов."""
    try:
        page.screenshot(
            path=str(out / f"{name}.jpg"), type="jpeg", quality=75, full_page=True
        )
    except Exception:
        pass

    lines = []
    if tab_label:
        lines.append(f"# Вкладка: {tab_label}\n")

    # текст основной страницы — без пустых ячеек грида и стрелок
    try:
        raw = page.locator("body").inner_text()
        cleaned = [s.strip() for s in raw.splitlines()
                   if s.strip() not in ("", "►", "◄")]
        lines.append("=== ТЕКСТ СТРАНИЦЫ ===")
        lines.append("\n".join(cleaned))
    except Exception as e:
        lines.append(f"(не смог прочитать текст страницы: {e})")

    # содержимое редакторов (описание неисправности / ремонта / диалог)
    for fr in page.frames:
        if fr == page.main_frame:
            continue
        try:
            txt = fr.locator("body").inner_text().strip()
        except Exception:
            continue
        if not txt or any(m in txt for m in _EDITOR_NOISE):
            continue
        lines.append("\n=== ТЕКСТ ИЗ РЕДАКТОРА ===")
        lines.append(txt)

    (out / f"{name}.txt").write_text("\n".join(lines), encoding="utf-8")


# --- точка входа -----------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Priority scraper (локальный запуск)")
    sub = parser.add_subparsers(dest="cmd", required=True)
    sub.add_parser("setup", help="залогиниться и сохранить сессию (один раз)")
    sub.add_parser("discover", help="изучить структуру ОДНОЙ заявки (все вкладки)")
    sub.add_parser("batch", help="пройти по заявкам, выгрузить вкладки, назвать по SC")
    sub.add_parser("run", help="выгрузить данные, используя сохранённую сессию")
    args = parser.parse_args()

    if args.cmd == "setup":
        cmd_setup()
    elif args.cmd == "discover":
        cmd_discover()
    elif args.cmd == "batch":
        cmd_batch()
    elif args.cmd == "run":
        cmd_run()


if __name__ == "__main__":
    main()
