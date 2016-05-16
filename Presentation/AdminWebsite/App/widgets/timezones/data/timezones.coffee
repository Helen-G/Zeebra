define (require) ->
    [
        { id: 1, useDaylightTime: 0, value: -12, text: "(UTC-12:00) International Date Line West" }
        { id: 2, useDaylightTime: 0, value: -11, text: "(UTC-11:00) Midway Island, Samoa" }
        { id: 3, useDaylightTime: 0, value: -10, text: "(UTC-10:00) Hawaii" }
        { id: 4, useDaylightTime: 1, value: -9, text: "(UTC-09:00) Alaska" }
        { id: 5, useDaylightTime: 1, value: -8, text: "(UTC-08:00) Pacific Time (US & Canada)" }
        { id: 6, useDaylightTime: 1, value: -8, text: "(UTC-08:00) Tijuana, Baja California" }
        { id: 7, useDaylightTime: 0, value: -7, text: "(UTC-07:00) Arizona" }
        { id: 8, useDaylightTime: 1, value: -7, text: "(UTC-07:00) Chihuahua, La Paz, Mazatlan" }
        { id: 9, useDaylightTime: 1, value: -7, text: "(UTC-07:00) Mountain Time (US & Canada)" }
        { id: 10, useDaylightTime: 0, value: -6, text: "(UTC-06:00) Central America" }
        { id: 11, useDaylightTime: 1, value: -6, text: "(UTC-06:00) Central Time (US & Canada)" }
        { id: 12, useDaylightTime: 1, value: -6, text: "(UTC-06:00) Guadalajara, Mexico City, Monterrey" }
        { id: 13, useDaylightTime: 0, value: -6, text: "(UTC-06:00) Saskatchewan" }
        { id: 14, useDaylightTime: 0, value: -5, text: "(UTC-05:00) Bogota, Lima, Quito, Rio Branco" }
        { id: 15, useDaylightTime: 1, value: -5, text: "(UTC-05:00) Eastern Time (US & Canada)" }
        { id: 16, useDaylightTime: 1, value: -5, text: "(UTC-05:00) Indiana (East)" }
        { id: 17, useDaylightTime: 1, value: -4, text: "(UTC-04:00) Atlantic Time (Canada)" }
        { id: 18, useDaylightTime: 0, value: -4, text: "(UTC-04:00) Caracas, La Paz" }
        { id: 19, useDaylightTime: 0, value: -4, text: "(UTC-04:00) Manaus" }
        { id: 20, useDaylightTime: 1, value: -4, text: "(UTC-04:00) Santiago" }
        { id: 21, useDaylightTime: 1, value: -3.5, text: "(UTC-03:30) Newfoundland" }
        { id: 22, useDaylightTime: 1, value: -3, text: "(UTC-03:00) Brasilia" }
        { id: 23, useDaylightTime: 0, value: -3, text: "(UTC-03:00) Buenos Aires, Georgetown" }
        { id: 24, useDaylightTime: 1, value: -3, text: "(UTC-03:00) Greenland" }
        { id: 25, useDaylightTime: 1, value: -3, text: "(UTC-03:00) Montevideo" }
        { id: 26, useDaylightTime: 1, value: -2, text: "(UTC-02:00) Mid-Atlantic" }
        { id: 27, useDaylightTime: 0, value: -1, text: "(UTC-01:00) Cape Verde Is." }
        { id: 28, useDaylightTime: 1, value: -1, text: "(UTC-01:00) Azores" }
        { id: 29, useDaylightTime: 0, value: 0, text: "(UTC+00:00) Casablanca, Monrovia, Reykjavik" }
        { id: 30, useDaylightTime: 1, value: 0, text: "(UTC+00:00) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London" }
        { id: 31, useDaylightTime: 1, value: 1, text: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna" }
        { id: 32, useDaylightTime: 1, value: 1, text: "(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague" }
        { id: 33, useDaylightTime: 1, value: 1, text: "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris" }
        { id: 34, useDaylightTime: 1, value: 1, text: "(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb" }
        { id: 35, useDaylightTime: 1, value: 1, text: "(UTC+01:00) West Central Africa" }
        { id: 36, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Amman" }
        { id: 37, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Athens, Bucharest, Istanbul" }
        { id: 38, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Beirut" }
        { id: 39, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Cairo" }
        { id: 40, useDaylightTime: 0, value: 2, text: "(UTC+02:00) Harare, Pretoria" }
        { id: 41, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius" }
        { id: 42, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Jerusalem" }
        { id: 43, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Minsk" }
        { id: 44, useDaylightTime: 1, value: 2, text: "(UTC+02:00) Windhoek" }
        { id: 45, useDaylightTime: 0, value: 3, text: "(UTC+03:00) Kuwait, Riyadh, Baghdad" }
        { id: 46, useDaylightTime: 1, value: 3, text: "(UTC+03:00) Moscow, St. Petersburg, Volgograd" }
        { id: 47, useDaylightTime: 0, value: 3, text: "(UTC+03:00) Nairobi" }
        { id: 48, useDaylightTime: 0, value: 3, text: "(UTC+03:00) Tbilisi" }
        { id: 49, useDaylightTime: 1, value:3.5, text: "(UTC+03:30) Tehran" }
        { id: 50, useDaylightTime: 0, value: 4, text: "(UTC+04:00) Abu Dhabi, Muscat" }
        { id: 51, useDaylightTime: 1, value: 4, text: "(UTC+04:00) Baku" }
        { id: 52, useDaylightTime: 1, value: 4, text: "(UTC+04:00) Yerevan" }
        { id: 53, useDaylightTime: 0, value:4.5, text: "(UTC+04:30) Kabul" }
        { id: 54, useDaylightTime: 1, value: 5, text: "(UTC+05:00) Yekaterinburg" }
        { id: 55, useDaylightTime: 0, value: 5, text: "(UTC+05:00) Islamabad, Karachi, Tashkent" }
        { id: 56, useDaylightTime: 0, value: 5.5, text: "(UTC+05:30) Sri Jayawardenapura" }
        { id: 57, useDaylightTime: 0, value: 5.5, text: "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi" }
        { id: 58, useDaylightTime: 0, value:5.75, text: "(UTC+05:45) Kathmandu" }
        { id: 59, useDaylightTime: 1, value: 6, text: "(UTC+06:00) Almaty, Novosibirsk" }
        { id: 60, useDaylightTime: 0, value: 6, text: "(UTC+06:00) Astana, Dhaka" }
        { id: 61, useDaylightTime: 0, value:6.5, text: "(UTC+06:30) Yangon (Rangoon)" }
        { id: 62, useDaylightTime: 0, value: 7, text: "(UTC+07:00) Bangkok, Hanoi, Jakarta" }
        { id: 63, useDaylightTime: 1, value: 7, text: "(UTC+07:00) Krasnoyarsk" }
        { id: 64, useDaylightTime: 0, value: 8, text: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi" }
        { id: 65, useDaylightTime: 0, value: 8, text: "(UTC+08:00) Kuala Lumpur, Singapore" }
        { id: 66, useDaylightTime: 0, value: 8, text: "(UTC+08:00) Irkutsk, Ulaan Bataar" }
        { id: 67, useDaylightTime: 0, value: 8, text: "(UTC+08:00) Perth" }
        { id: 68, useDaylightTime: 0, value: 8, text: "(UTC+08:00) Taipei" }
        { id: 69, useDaylightTime: 0, value: 9, text: "(UTC+09:00) Osaka, Sapporo, Tokyo" }
        { id: 70, useDaylightTime: 0, value: 9, text: "(UTC+09:00) Seoul" }
        { id: 71, useDaylightTime: 1, value: 9, text: "(UTC+09:00) Yakutsk" }
        { id: 72, useDaylightTime: 0, value:9.5, text: "(UTC+09:30) Adelaide" }
        { id: 73, useDaylightTime: 0, value:9.5, text: "(UTC+09:30) Darwin" }
        { id: 74, useDaylightTime: 0, value: 10, text: "(UTC+10:00) Brisbane" }
        { id: 75, useDaylightTime: 1, value: 10, text: "(UTC+10:00) Canberra, Melbourne, Sydney" }
        { id: 76, useDaylightTime: 1, value: 10, text: "(UTC+10:00) Hobart" }
        { id: 77, useDaylightTime: 0, value: 10, text: "(UTC+10:00) Guam, Port Moresby" }
        { id: 78, useDaylightTime: 1, value: 10, text: "(UTC+10:00) Vladivostok" }
        { id: 79, useDaylightTime: 1, value: 11, text: "(UTC+11:00) Magadan, Solomon Is., New Caledonia" }
        { id: 80, useDaylightTime: 1, value: 12, text: "(UTC+12:00) Auckland, Wellington" }
        { id: 81, useDaylightTime: 0, value: 12, text: "(UTC+12:00) Fiji, Kamchatka, Marshall Is." }
        { id: 82, useDaylightTime: 0, value: 13, text: "(UTC+13:00) Nuku'alofa" }
    ]