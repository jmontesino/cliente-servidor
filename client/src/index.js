import repository from "./repository.js";
import TIPO_EVENTO from "./event_type.js";

const refreshButton = document.getElementById("sincronizar-btn");
const eventosCount = document.getElementById("eventos-count");
const desincronizadosCount = document.getElementById("desincronizados-count");

refreshButton.addEventListener("click", async () => {
    await repository.refreshEventosAsync();
    await loadEventosAsync();
});

async function loadEventosAsync() {
    try {
        const eventos = await repository.getAllEventosAsync();
        eventosCount.innerText = eventos.length;
        const tbody = document.getElementById("eventosBody");
        tbody.innerHTML = "";
        const desincronizados = eventos.filter((e) => e.estado !== TIPO_EVENTO.Sincronizado);
        desincronizadosCount.innerText = desincronizados.length;
        if (desincronizados.length == 0) {
            refreshButton.disabled = true;
        } else {
            refreshButton.disabled = false;
        }

        eventos.forEach((evento) => {
            const color = evento.estado === TIPO_EVENTO.Sincronizado ? "green" : "red";
            const row = tbody.insertRow();
            row.className =
                evento.estado === TIPO_EVENTO.Sincronizado
                    ? "bg-green-100 border-l-4 border-green-500 hover:bg-green-200"
                    : "bg-red-100 border-l-4 border-red-500 hover:bg-red-200";
            row.innerHTML = `
            <td>${evento.id}</td>
            <td>${evento.identificacion}</td>
            <td>${evento.nombre}</td>
            <td>${evento.tipo}</td>
            <td>${new Date(evento.fecha).toLocaleString()}</td>
            <td>${evento.estado}</td>
          `;
        });
    } catch (error) {
        console.error("Error cargando eventos:", error);
    }
}

loadEventosAsync();
