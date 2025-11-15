import TIPO_EVENTO from "./event_type.js";
import HTTP_SERVICE from "./http_service.js";

const DB_NAME = "Visitantes";
const DB_VERSION = 1;
const STORE_NAME = "eventos";
const SEDE_KEY = "sede";

class Repository {
    #db = null;

    constructor(httpService) {
        this.httpService = httpService;
    }

    async addEventoAsync(evento) {
        await this.#putDBAsync(evento);
        try {
            await this.httpService.putAsync(`/eventos`, evento);
            evento.estado = TIPO_EVENTO.Sincronizado;
            await this.#putDBAsync(evento);
        } catch (error) {
            console.error("Error al conectar con la API:", error);
        }
    }

    async refreshEventosAsync() {
        const eventos = await this.#getAllEventosDBAsync();
        const desincronizados = eventos.filter(
            (e) => e.estado !== TIPO_EVENTO.Sincronizado,
        );
        for (const evento of desincronizados) {
            try {
                await this.httpService.putAsync("/eventos", evento);
            } catch (error) {
                console.error("Error al conectar con la API:", error);
            }
        }
    }

    async getAllEventosAsync() {
        try {
            const response = await this.httpService.getAsync(
                `/eventos/${localStorage.getItem(SEDE_KEY)}`,
            );
            for (const evento of response) {
                evento.estado = TIPO_EVENTO.Sincronizado;
                await this.#putDBAsync(evento);
            }
        } catch (error) {
            console.error("Error al procesar eventos de la API:", error);
        }

        var eventos = this.#getAllEventosDBAsync();
        return eventos;
    }

    async #openConnectionAsync() {
        if (this.#db) {
            return this.#db;
        }

        return new Promise((resolve, reject) => {
            const request = indexedDB.open(DB_NAME, DB_VERSION);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                this.#db = request.result;
                resolve(this.#db);
            };

            request.onupgradeneeded = (event) => {
                const newDB = event.target.result;
                if (!newDB.objectStoreNames.contains(STORE_NAME)) {
                    newDB.createObjectStore(STORE_NAME, {
                        keyPath: "id",
                        autoIncrement: true,
                    });
                }
            };
        });
    }

    async #putDBAsync(evento) {
        const db = await this.#openConnectionAsync();
        const tx = db.transaction([STORE_NAME], "readwrite");
        const store = tx.objectStore(STORE_NAME);
        return new Promise((resolve, reject) => {
            const request = store.put(evento);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async #getAllEventosDBAsync() {
        const db = await this.#openConnectionAsync();
        const tx = db.transaction([STORE_NAME], "readonly");
        const store = tx.objectStore(STORE_NAME);
        return new Promise((resolve, reject) => {
            const request = store.getAll();
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }
}

const repository = new Repository(HTTP_SERVICE);

export default repository;
