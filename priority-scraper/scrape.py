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
        browser = pw.chromium.launch(headless=False)
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
        browser = pw.chromium.launch(headless=HEADLESS)
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


# --- точка входа -----------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Priority scraper (локальный запуск)")
    sub = parser.add_subparsers(dest="cmd", required=True)
    sub.add_parser("setup", help="залогиниться и сохранить сессию (один раз)")
    sub.add_parser("run", help="выгрузить данные, используя сохранённую сессию")
    args = parser.parse_args()

    if args.cmd == "setup":
        cmd_setup()
    elif args.cmd == "run":
        cmd_run()


if __name__ == "__main__":
    main()
