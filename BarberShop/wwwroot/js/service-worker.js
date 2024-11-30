self.addEventListener('push', event => {
    const data = event.data.json();

    self.registration.showNotification(data.title, {
        body: data.body,
        icon: data.icon || '/favicon.ico', // Ícone padrão
        data: data.url // URL para abrir ao clicar
    });
});

self.addEventListener('notificationclick', event => {
    event.notification.close();

    if (event.notification.data) {
        clients.openWindow(event.notification.data); // Abre a URL associada
    }
});
