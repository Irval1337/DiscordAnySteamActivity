## DiscordAnySteamActivity
Утилита, позволяющая улучшить отображение любых приложений из Steam в игровой активности Discord (Rich Presence). При использовании добавляет отображение иконки текущей игры/программы, режим бездействия, а также частично изменяет отображение времени активности. Для разработчиков присутствует возможность модификации остальных параметров RPC.

### Установка
Перед началом использования ПО установите .NET Framework с официального сайта Microsoft - https://dotnet.microsoft.com/en-us/download/dotnet/6.0. <br/>
Создайте (если отсутствует) в директории с .exe файл `settings.json` со следующим форматом:<br/>
```json
{
  "steam_api_token": "Токен от ВАШЕГО аккаунта. Получать https://steamcommunity.com/dev/apikey",
  "discord_request_settings": {
    "authorization": "Загрузите Asset в своем приложении и отловите http запрос с помощью Ctrl + Shift + I -> Сеть. Поля - заголовки запроса",
    "cookie": "",
    "user_agent": "",
    "sec_ch_ua": "Кавычки из заголовка пишите как \"",
    "sec_ch_ua_mobile": "",
    "sec_ch_ua_platform": "Кавычки из заголовка пишите как \"",
    "sec_ch_ua_dest": "",
    "sec_ch_ua_mode": "",
    "sec_ch_ua_site": ""
  },
  "discord_application_settings": { 
    "app_id": "Создайте приложение на сайте https://discord.com/developers/applications. APPLICATION ID", 
    "assets": null,
    "idling_image": "Загрузите изображение для бездействия. Укажите в данном поле название файла"
  },
  "update_delay": 50000
}
```

### Динамическая загрузка Assets
Для динамической подгрузки изображений в Discord Developer Portal используется симулирование прямых действий пользователя, из-за чего требуется сохранить в файле настроек хедеры оригинального запроса. Ограничения от Discord, связанные с кэшированием изображений, влияют на время загрузки изображения новой игры. После первичного запуска требуется от 5 до 15 минут для отображения картинки у всех в активности.

### Пример работы
Без программы<br/> ![image](https://user-images.githubusercontent.com/56792892/178103194-29688bc6-9567-46c1-8e10-b3cd0f3b5941.png) <br/>
С программой <br/>![image](https://user-images.githubusercontent.com/56792892/178103230-0a583a48-de01-4e33-bff6-417c86b8ceff.png)

