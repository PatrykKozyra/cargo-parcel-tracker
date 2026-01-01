// SignalR Client for Real-Time Parcel Updates
// Demonstrates .NET's superior WebSocket integration vs Django Channels

(function () {
    'use strict';

    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/parcelStatus")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Connection event handlers
    connection.onreconnecting((error) => {
        console.warn('SignalR connection lost. Reconnecting...', error);
        showToast('Connection lost. Reconnecting...', 'warning');
    });

    connection.onreconnected((connectionId) => {
        console.log('SignalR reconnected. Connection ID:', connectionId);
        showToast('Connection restored!', 'success');
    });

    connection.onclose((error) => {
        console.error('SignalR connection closed:', error);
        showToast('Real-time updates disconnected', 'danger');
    });

    // Listen for connection confirmation
    connection.on("Connected", (message) => {
        console.log('SignalR connected:', message);
        showToast(message, 'success', 3000);
    });

    // Listen for new parcel creation
    connection.on("NewParcelCreated", (data) => {
        console.log('New parcel created:', data);

        showToast(
            `📦 New Parcel Created by ${data.userName}<br>
            <strong>${data.parcelName}</strong><br>
            ${data.crudeGrade} - ${data.quantity.toLocaleString()} bbls`,
            'info',
            5000
        );

        // Refresh the parcel list if we're on the index page
        if (window.location.pathname.toLowerCase().includes('/cargoparcels')) {
            setTimeout(() => {
                if (confirm('A new parcel was created. Refresh the page to see it?')) {
                    window.location.reload();
                }
            }, 1000);
        }
    });

    // Listen for parcel status changes
    connection.on("ParcelStatusChanged", (data) => {
        console.log('Parcel status changed:', data);

        const statusColors = {
            'Planned': 'secondary',
            'Confirmed': 'primary',
            'Loading': 'warning',
            'InTransit': 'info',
            'Discharged': 'success',
            'Cancelled': 'danger'
        };

        const newStatusColor = statusColors[data.newStatus] || 'secondary';

        showToast(
            `🔄 Status Update by ${data.userName}<br>
            <strong>${data.parcelName}</strong><br>
            ${data.oldStatus} → <span class="badge bg-${newStatusColor}">${data.newStatus}</span>`,
            'warning',
            5000
        );

        // Update the badge on the page if the parcel row exists
        updateParcelStatusBadge(data.parcelId, data.newStatus, newStatusColor);
    });

    // Listen for parcel deletion
    connection.on("ParcelDeleted", (data) => {
        console.log('Parcel deleted:', data);

        showToast(
            `🗑️ Parcel Deleted by ${data.userName}<br>
            <strong>${data.parcelName}</strong>`,
            'danger',
            4000
        );

        // Remove the row from the table if it exists
        removeParcelRow(data.parcelId);
    });

    // Start the connection
    connection.start()
        .then(() => {
            console.log('SignalR connected successfully');
        })
        .catch((error) => {
            console.error('SignalR connection error:', error);
            showToast('Failed to connect for real-time updates', 'danger');
        });

    // Helper function to show toast notifications
    function showToast(message, type = 'info', duration = 4000) {
        // Remove any existing toasts
        const existingToast = document.getElementById('signalr-toast');
        if (existingToast) {
            existingToast.remove();
        }

        // Create toast element
        const toast = document.createElement('div');
        toast.id = 'signalr-toast';
        toast.className = `toast-notification toast-${type}`;
        toast.innerHTML = `
            <div class="toast-header">
                <strong>Real-Time Update</strong>
                <button type="button" class="btn-close-toast" onclick="this.parentElement.parentElement.remove()">
                    ×
                </button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;

        document.body.appendChild(toast);

        // Trigger animation
        setTimeout(() => toast.classList.add('show'), 10);

        // Auto-remove after duration
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, duration);
    }

    // Helper function to update parcel status badge in the table
    function updateParcelStatusBadge(parcelId, newStatus, colorClass) {
        const row = document.querySelector(`tr[data-parcel-id="${parcelId}"]`);
        if (row) {
            const statusBadge = row.querySelector('.parcel-status-badge');
            if (statusBadge) {
                // Remove all bg-* classes
                statusBadge.className = statusBadge.className.replace(/bg-\w+/g, '');
                // Add new color class
                statusBadge.classList.add(`bg-${colorClass}`);
                // Update text
                statusBadge.textContent = newStatus;

                // Add pulse animation
                row.classList.add('pulse-row');
                setTimeout(() => row.classList.remove('pulse-row'), 2000);
            }
        }
    }

    // Helper function to remove parcel row from table
    function removeParcelRow(parcelId) {
        const row = document.querySelector(`tr[data-parcel-id="${parcelId}"]`);
        if (row) {
            row.classList.add('fade-out-row');
            setTimeout(() => row.remove(), 500);
        }
    }

    // Export connection for potential use elsewhere
    window.signalRConnection = connection;
})();
