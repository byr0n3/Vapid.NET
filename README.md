# Vapid.NET

A minimal, no dependency Vapid WebPush client for .NET.

## Motivation

[The original/most popular WebPush client for .NET](https://github.com/web-push-libs/web-push-csharp) has multiple
issues (in my opinion):

- It uses the `Portable.BouncyCastle` package.
	- This package was made before .NET had built-in Cryptography functions that WebPush requires.
	- There is [a pull request](https://github.com/web-push-libs/web-push-csharp/pull/75) that tries to fix this,
	  however, this code uses Windows-only functions and hasn't been updated to resolve this issue.
- It uses the `Newtonsoft.Json` package.
	- Again, this package was made before .NET had efficient built-in JSON parsing.
	- The (current) latest `master` branch has received a commit to remove this package, however, the package hasn't
	  been updated [since July 2021](https://github.com/web-push-libs/web-push-csharp/releases/tag/v1.0.12).
- It *can* allocate quite some data in memory.
- You are almost required to write a wrapper around this package to make it more developer-friendly.

## Features

- [x] Lightweight package that has little to no dependencies
- [x] Faster than [web-push-csharp](https://github.com/web-push-libs/web-push-csharp) when creating HTTP requests.
	- The speed of this library is almost completely dependent on the speed of the WebPush server.
- [x] [Declarative Web Push](https://github.com/WebKit/explainers/blob/main/DeclarativeWebPush/README.md) support.
- [x] Clean and developer-friendly API.

## Missing features

- [ ] No GCM support.
	- Even though GCM support seems redundant, an application might still need it.

## Usage

### ASP.NET Core: Server

1. Configure Vapid options and register the Vapid client for DI:

```csharp
using Vapid.NET;

services.Configure<VapidOptions>((options) =>
{
	// You can generate this information on the following website:
	// https://vapidkeys.com

	options.Subject = "…"; // Most of the time, this is an `mailto:…` address.
	options.PublicKey = "…";
	options.PrivateKey = "…";
});

services.AddHttpClient<VapidClient>("Vapid WebPush");
```

2. Send a push notification:

```csharp
using Vapid.NET;
using Vapid.NET.Models;

// Don't forget to inject the `VapidClient`.

// You'd probably store and fetch this from a database:
var pushSubscription = new PushSubscription
{
	Endpoint = "…",
	P256dh = "…",
	Auth = "…",
};

var notification = new PushNotification
{
	Title = "Vapid.NET",
	Body = "Hello from the server!",
	Navigate = "https://vapid.net/docs",
};

// `SendAsync` returns a boolean indicating if the push notification was sent.
var sent = await client.SendAsync(pushSubscription, notification, /*optional:*/ cancellationToken);
```

### ASP.NET Core: Client

Push notifications are sent using the [Declarative Web
Push](https://github.com/WebKit/explainers/blob/main/DeclarativeWebPush/README.md) format. But not a lot of browsers
support this format (yet). You should make sure your application has a service worker that handles the `push` event:

```js
self.addEventListener('push', function (event) {
	event.waitUntil(showNotification(event));
});

self.addEventListener('notificationclick', function (event) {
	event.notification.close();

	event.waitUntil(onNotificationClick(event));
});

async function showNotification(event) {
	if (!event.data) {
		throw new Error('Received push event without any data.');
	}

	const data = await event.data.json();
	const notification = data.notification;

	await self.registration.showNotification(notification.title, {
		icon: '/icon-512.png',
		lang: notification.lang,
		dir: notification.dir,
		body: notification.body,
		silent: notification.silent,
		tag: notification.topic,
		data: {
			navigate: notification.navigate,
		}
	});
}

async function onNotificationClick(event) {
	const notification = event.notification;

	const url = notification.data.navigate;

	const windows = await clients.matchAll({type: 'window'});

	for (const client of windows) {
		if (client.url === url && 'focus' in client) {
			return await client.focus();
		}
	}

	if (clients.openWindow) {
		return clients.openWindow(url);
	}
}
```
