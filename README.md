GNU GPL v3 license — you can do anything you want, but you must give me credit and distribute your changes under the same terms.

MailAgent is an IMAP email monitoring service implemented as a module for the OutfitTool ecosystem. The application periodically checks configured email accounts for unread messages and provides system notifications through the taskbar and notification center.

Key features:

- **IMAP integration**: Connects to email servers via IMAP with SSL/TLS encryption using MailKit library
- **Multi-account support**: Manages multiple email accounts with individual server, port, and authentication settings
- **Secure credential storage**: Encrypts passwords using Windows DPAPI with PROCESSOR_IDENTIFIER env variable as additional entropy
- **Background polling**: Configurable check interval (default: 5 seconds) with automatic reconnection after configurable timeout
- **Taskbar integration**: Displays unread message count in taskbar icon tooltip and shows email notifications
- **Account management**: GUI for adding, editing, and removing email accounts with password encryption

The application is implemented as an OutfitTool API1 module with ModuleController handling lifecycle management. The MailService class manages IMAP connections using ImapClient, caching message states and filtering system folders (Drafts, Junk, Sent, Trash, Archive). Notifications include sender, subject, and mail reference, with pluralization support for different languages. Settings are persisted via MailAgentSettings and AccountSettings classes using the API's SettingsManager, with encrypted password storage through the Crypter utility class.

---

Лицензия GNU GPL v3 — вы можете делать с этим что угодно, но должны указывать авторство и распространять ваши изменения на тех же условиях.

MailAgent — это служба мониторинга электронной почты по протоколу IMAP, реализованная как модуль для экосистемы OutfitTool. Приложение периодически проверяет настроенные почтовые аккаунты на наличие непрочитанных сообщений и предоставляет системные уведомления через панель задач и центр уведомлений.

Ключевые особенности:

- **Интеграция с IMAP**: Подключение к почтовым серверам по протоколу IMAP с шифрованием SSL/TLS с использованием библиотеки MailKit
- **Поддержка нескольких аккаунтов**: Управление несколькими почтовыми аккаунтами с индивидуальными настройками сервера, порта и аутентификации
- **Безопасное хранение учетных данных**: Шифрование паролей с использованием Windows DPAPI с переменной окружения PROCESSOR_IDENTIFIER в качестве дополнительной энтропии
- **Фоновая проверка**: Настраиваемый интервал проверки (по умолчанию: 5 секунд) с автоматическим переподключением после настраиваемого таймаута
- **Интеграция с панелью задач**: Отображение количества непрочитанных сообщений во всплывающей подсказке значка на панели задач и показ уведомлений о письмах
- **Управление аккаунтами**: Графический интерфейс для добавления, редактирования и удаления почтовых аккаунтов с шифрованием паролей

Приложение реализовано как модуль OutfitTool API1, где ModuleController управляет жизненным циклом. Класс MailService управляет подключениями IMAP с использованием ImapClient, кэширует состояния сообщений и фильтрует системные папки (Черновики, Спам, Отправленные, Корзина, Архив). Уведомления включают отправителя, тему и ссылку на письмо, с поддержкой склонения для разных языков. Настройки сохраняются через классы MailAgentSettings и AccountSettings с использованием SettingsManager API, с шифрованным хранением паролей через вспомогательный класс Crypter.
