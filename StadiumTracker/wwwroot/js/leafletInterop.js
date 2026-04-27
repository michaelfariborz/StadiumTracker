const leafletInterop = (() => {
    let map = null;
    let markerLayer = null;
    let dotNetRef = null;

    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
    }

    function makeIcon(visited) {
        return L.divIcon({
            className: '',
            html: `<svg xmlns="http://www.w3.org/2000/svg" width="28" height="40" viewBox="0 0 28 40">
                     <ellipse cx="14" cy="38" rx="6" ry="2.5" fill="rgba(0,0,0,0.2)"/>
                     <path d="M14 0 C6.27 0 0 6.27 0 14 C0 24.5 14 40 14 40 C14 40 28 24.5 28 14 C28 6.27 21.73 0 14 0 Z"
                           fill="${visited ? '#4ade80' : '#d1d5db'}"
                           stroke="${visited ? '#166534' : '#6b7280'}" stroke-width="2"/>
                     <circle cx="14" cy="14" r="6" fill="${visited ? '#166534' : '#9ca3af'}"/>
                   </svg>`,
            iconSize: [28, 40],
            iconAnchor: [14, 40],
            popupAnchor: [0, -42]
        });
    }

    function buildPopupHtml(s) {
        let html = `<strong>${escapeHtml(s.name)}</strong><br>
                    <em>${escapeHtml(s.homeTeam)}</em> &mdash; ${escapeHtml(s.city)}, ${escapeHtml(s.state)}<br><br>`;
        if (s.visited && s.visits && s.visits.length > 0) {
            html += '<strong>Your visits:</strong><ul style="margin:4px 0 8px 0;padding-left:16px;">';
            for (const v of s.visits) {
                const date = v.visitDate ? new Date(v.visitDate).toLocaleDateString() : 'Date not recorded';
                const opp  = v.opponentTeam ? ` vs ${escapeHtml(v.opponentTeam)}` : '';
                const score = v.score ? ` &mdash; ${escapeHtml(v.score)}` : '';
                html += `<li>${date}${opp}${score}</li>`;
            }
            html += '</ul>';
        }
        html += `<button class="btn btn-sm btn-primary" onclick="leafletInterop.triggerAddVisit(${s.id})">+ Add Visit</button>`;
        return html;
    }

    return {
        initMap(containerId, lat, lon, zoom) {
            const el = document.getElementById(containerId);
            if (!el) return;
            if (map) { map.remove(); map = null; }
            dotNetRef = null;
            map = L.map(containerId).setView([lat, lon], zoom);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
                maxZoom: 19
            }).addTo(map);
            markerLayer = L.layerGroup().addTo(map);
        },

        addPins(stadiums, ref) {
            dotNetRef = ref;
            if (!markerLayer) return;
            markerLayer.clearLayers();
            for (const s of stadiums) {
                const marker = L.marker([s.latitude, s.longitude], { icon: makeIcon(s.visited) });
                marker.bindPopup(buildPopupHtml(s), { maxWidth: 300 });
                markerLayer.addLayer(marker);
            }
        },

        clearPins() {
            if (markerLayer) markerLayer.clearLayers();
            dotNetRef = null;
        },

        triggerAddVisit(stadiumId) {
            if (dotNetRef) dotNetRef.invokeMethodAsync('OpenAddVisitFromMap', stadiumId);
        }
    };
})();

window.leafletInterop = leafletInterop;
