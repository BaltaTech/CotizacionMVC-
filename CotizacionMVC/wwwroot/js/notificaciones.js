// wwwroot/js/notificaciones.js

let connection = null;
let badgePendientes = 0;

// Iniciar conexión SignalR
function iniciarSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificacionHub")
        .withAutomaticReconnect()
        .build();

    // Recibir notificación
    connection.on("RecibirNotificacion", function (notificacion) {
        mostrarNotificacion(notificacion);
        actualizarBadge();
        reproducirSonido();
    });

    // Iniciar conexión
    connection.start()
        .then(function () {
            console.log("SignalR conectado");
        })
        .catch(function (err) {
            console.error("Error SignalR:", err);
            setTimeout(iniciarSignalR, 5000); // Reintentar
        });

    // Reconexión
    connection.onreconnecting(function () {
        console.log("Reconectando SignalR...");
    });

    connection.onreconnected(function () {
        console.log("SignalR reconectado");
    });
}

// Mostrar notificación toast
function mostrarNotificacion(notificacion) {
    const colores = {
        success: '#28a745',
        warning: '#ffc107',
        danger: '#dc3545',
        info: '#17a2b8'
    };

    const toast = `
        <div class="toast-notificacion" style="
            position: fixed;
            top: 20px;
            right: 20px;
            background: white;
            border-left: 4px solid ${colores[notificacion.tipo] || colores.info};
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
            padding: 15px 20px;
            border-radius: 8px;
            z-index: 99999;
            min-width: 300px;
            animation: slideIn 0.3s ease;
        ">
            <div style="display: flex; justify-content: space-between; align-items: start;">
                <div>
                    <strong>${notificacion.titulo}</strong>
                    <p style="margin: 5px 0 0 0; color: #666;">${notificacion.mensaje}</p>
                    <small style="color: #999;">${notificacion.fecha}</small>
                </div>
                <button onclick="this.parentElement.parentElement.remove()" 
                    style="background: none; border: none; font-size: 18px; cursor: pointer; color: #999;">
                    ×
                </button>
            </div>
        </div>
    `;

    document.body.insertAdjacentHTML('beforeend', toast);

    // Auto-eliminar después de 5 segundos
    setTimeout(function () {
        const toasts = document.querySelectorAll('.toast-notificacion');
        if (toasts.length > 0) {
            toasts[0].remove();
        }
    }, 5000);
}

// Actualizar badge de pendientes
function actualizarBadge() {
    badgePendientes++;
    const badge = document.getElementById('badgePendientes');
    if (badge) {
        badge.textContent = badgePendientes;
        badge.style.display = 'inline-block';
    }
}

// Reproducir sonido de notificación
function reproducirSonido() {
    try {
        const audio = new Audio('/sounds/notification.mp3');
        audio.volume = 0.3;
        audio.play();
    } catch (e) {
        // Sin sonido si no existe el archivo
    }
}

// Iniciar al cargar la página
document.addEventListener('DOMContentLoaded', function () {
    iniciarSignalR();
});

// Animación CSS para los toasts
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
`;
document.head.appendChild(style);