# Школа предпринимательства - сервер

## Использованные технологии

Проект написан на ASP.NET на языке C#. Для запросов к базе данных использован Entity Framework. Для отправки писем использована библиотека MailKit.

## Структура проекта

В папке `Models` лежат модели таблиц базы данных. В папке `DTOs` находятся объекты для приёма запросов и отправки ответов в формате json. В `Controllers` находятся контроллеры, разбитые по тематике, обрабатывающие запросы, касающиеся разных частей проекта. В папке `Utility` лежат статические классы, предоставляющие утилитарные функции. Класс `CheckDeadlineJob` отвечает за процесс, отправляющий почтовую рассылку при приближающемся дедлайне.

## Конфигурация проекта

В файле `Configure.cs` находятся параметры конфигурации проекта. Там можно поменять логин/пароль от администратора, включить/выключить почтовую рассылку, настроить параметры аккаунта для почтовой рассылки. 

### Настройка почтовой рассылки. 

Проект использует подключение по SMTP. В частности, для Яндекс.Почты используется вход по [паролю приложения](https://yandex.ru/support/id/authorization/app-passwords.html). В файле `Configure.cs` можно увидеть настройки адреса и порта подключения к почте, логина и пароля, а также шаблонных тем и сообщений рассылок. 

## Запуск приложения

В проект запускается путём поднятия двух docker контейнеров. Описание контейнера серверного приложения находится в файле `Dockerfile`. Для доступа к базе данных используется контейнер с PostgreSQL. Описание настроект контейнеров, в том числе user, password и имя базы данных находятся в файле `docker-compose.yml` и `docker-compose.dev.yml`. Рекомендуется для запуска на продакшен сервере использовать команду 

    docker-compose up -d

в директории c файлом `docker-compose.yml`.

Для запуска во время разработки рекомендуется использовать файл `docker-compose.dev.yml`, так как в нём открыт порт к базе данных, что позволяет подключиться к ней во время разработки. 

После внесения изменения в код надо выполнить следующую последовательность команд:

    docker-compose build backend;
    docker-compose up -d --force-recreate;
    docker system prune --all --force;

Без выполнения этой последовательности команд docker возмёт уже собранный образ сервера, и внесенные изменения не будут задействованы. 

## CD

у проекта настроен автоматический деплой на сервер с использованием GitHub Actions по ssh. Параметры сервера находятся в разделе `Secrets` репозитория, а скрипт находится в `.github/workflows/deploy.yml`.

