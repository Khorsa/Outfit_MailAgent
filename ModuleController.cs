/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using System.Globalization;
using System.Timers;
using System.Windows.Media.Imaging;
using MailAgent.Settings;
using MailKit;
using OutfitTool.API1;
using OutfitTool.API1.Dto;
using OutfitTool.API1.Logger;
using Timer = System.Timers.Timer;

namespace MailAgent;

internal class ModuleController : IModuleController
{
    private Timer? _timer;

    private List<MailDto> _unreadMessages = new();
    private readonly Dictionary<string, MailDto> _notifiedMessages = new();

    private static BitmapImage? _mailIcon;

    private readonly MailService _mailService = new();
    private readonly SettingsManager<MailAgentSettings> _settingManager = new();
    private ILogger? _logger;

    public List<ICommand> GetCommandList()
    {
        return [];
    }

    public void SetLanguage(string language)
    {
        LocalizationHelper.SetLanguage(new CultureInfo(language));
    }

    public void Shutdown()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Elapsed -= OnTimedEvent;
            _timer.Dispose();
            _timer = null;
        }

        _mailService.Dispose();
    }

    private void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        try
        {
            var settings = _settingManager.LoadSettings(force: true);
            var tempUnreadMessages = new List<MailDto>();
            foreach (var accountSettings in settings.GetAccountSettings())
            {
                try
                {
                    var unreadAccountMessages = _mailService.CheckMail(accountSettings, settings.TimeToReconnect);
                    if (unreadAccountMessages == null) continue;
                    tempUnreadMessages.AddRange(unreadAccountMessages);
                }
                catch (Exception ex) {
                    _logger?.Error(ex.Message);
                }
            }

            // Здесь переносим сообщения в unreadMessages (для обеспечения атомарности операции)
            _unreadMessages = tempUnreadMessages;
        }
        catch (Exception ex) {
            _logger?.Error(ex.ToString());
        }
    }

    public BitmapImage? GetTaskbarIcon()
    {
        if (_unreadMessages.Count == 0)
        {
            return null;
        }

        return _mailIcon ??= new BitmapImage(new Uri("pack://application:,,,/MailAgent;component/Resources/mail.ico"));
    }

    public string? GetTaskbarIconText()
    {
        if (_unreadMessages.Count == 0)
        {
            return null;
        }
        return "непрочитанных - " + _unreadMessages.Count;
    }

    public Notification? PopNotification()
    {
        var newUnreadMessages = new List<MailDto>();
        foreach (var unreadMessage in _unreadMessages)
        {
            if (!_notifiedMessages.ContainsKey(unreadMessage.Key))
            {
                newUnreadMessages.Add(unreadMessage);
            }
        }

        Notification? notification = null;

        if (newUnreadMessages.Count == 1)
        {
            var last = newUnreadMessages[^1];
            notification = new Notification(last.To + ": <" + last.From + ">", last.Subject, last.MailRef);
        }
        else if (newUnreadMessages.Count > 1)
        {
            var last = newUnreadMessages[^1];
            notification = new Notification(last.To + ":",
                GetNotificationMessage(newUnreadMessages.Count), last.MailRef);
        }

        // Запоминаем только актуальные письма, чтобы словарь не рос бесконечно.
        var currentKeys = new HashSet<string>(_unreadMessages.Select(message => message.Key));
        foreach (var staleKey in _notifiedMessages.Keys.Where(key => !currentKeys.Contains(key)).ToList())
        {
            _notifiedMessages.Remove(staleKey);
        }

        foreach (var unreadMessage in _unreadMessages)
        {
            _notifiedMessages[unreadMessage.Key] = unreadMessage;
        }

        return notification;
    }

    private static string GetNotificationMessage(int count)
    {
        var (lastDigit, lastTwoDigits) = (count % 10, count % 100);
    
        string messageEnding = (lastDigit == 1 && lastTwoDigits != 11) 
            ? LocalizationHelper.GetString("NewMessageNominativeCase")
            : (lastDigit is >= 2 and <= 4 && lastTwoDigits is < 12 or > 14)
                ? LocalizationHelper.GetString("NewMessageGenitiveSingular")
                : LocalizationHelper.GetString("NewMessageGenitivePlural");
    
        return $"{LocalizationHelper.GetString("YouHave")} {count} {messageEnding}";
    }

    public void OpenSettings()
    {
        var settings = _settingManager.LoadSettings(true);
        var form = new SettingsForm(settings);
        if (form.ShowDialog() != true) return;
        _settingManager.SaveSettings(settings);
    }

    public void Init(ILogger logger)
    {
        _logger = logger;
        _mailService.Logger = logger;
        var settings = _settingManager.LoadSettings(force: true);

        _timer?.Stop();
        _timer?.Dispose();

        _timer = new Timer(Math.Max(1, settings.CheckInterval) * 1000);
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }
}