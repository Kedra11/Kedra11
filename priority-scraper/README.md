# Priority scraper

Скрипт запускается **у тебя на машине** (там, где есть доступ к внутреннему
Priority), заходит в веб-интерфейс и выгружает данные в CSV/Excel.

Пароль и внутренний адрес хранятся только в локальном `.env` — в репозиторий
они не попадают.

## Установка (один раз)

Нужен Python 3.9+.

```bash
cd priority-scraper

# 1. виртуальное окружение (по желанию, но так чище)
python -m venv .venv
# Windows:
.venv\Scripts\activate
# macOS/Linux:
source .venv/bin/activate

# 2. зависимости
pip install -r requirements.txt

# 3. браузер для Playwright
playwright install chromium
```

## Настройка

```bash
cp .env.example .env      # Windows: copy .env.example .env
```

Открой `.env` и впиши:

- `PRIORITY_URL` — твой внутренний адрес (`https://fbc-priority.xxxx.internal/`)
- `PRIORITY_USER` / `PRIORITY_PASS` — логин/пароль (можно оставить пустыми и
  залогиниться руками при setup)

## Запуск

```bash
# 1) первый раз — залогиниться и сохранить сессию
python scrape.py setup

# 2) дальше — выгружать данные
python scrape.py run
```

Результаты появятся в папке `output/`.

## Как «научить» скрипт нужному экрану

Я (Claude) не вижу твои экраны Priority, поэтому точные клики впишем по одному
из двух путей:

1. **Codegen (проще всего).** Запусти:
   ```bash
   playwright codegen ТВОЙ_PRIORITY_URL
   ```
   Кликай мышкой по нужному отчёту — Playwright сам напишет код клика.
   Пришли мне этот код, и я вставлю его в `scrape.py`.

2. **Опиши словами**, какой отчёт/экран нужен и какие данные с него забрать —
   я напишу навигацию сам, а ты подгонишь селекторы при первом запуске.

> 💡 В Priority обычно есть встроенная кнопка **«Экспорт в Excel»**. Нажать её
> и поймать скачивание — самый надёжный способ (см. `download_via_click()` в
> `scrape.py`). Если она есть — используем её.
