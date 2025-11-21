import TIPO_EVENTO from "./event_type.js";
import HTTP_SERVICE from "./http_service.js";

const DB_NAME = "Bellavista";
const DB_VERSION = 1;
const EVENTOS_STORE_NAME = "eventos";
const SEDES_STORE_NAME = "sedes";
const SEDE_KEY = "sede";

class Repository {
    #db = null;

    constructor(httpService) {
        this.httpService = httpService;
    }

    async addSedeAsync(sede) {
        await this.#putSedeDBAsync(sede);
        try {
            await this.httpService.putAsync(`/sedes`, sede);
            sede.estado = TIPO_EVENTO.Sincronizado;
            await this.#putSedeDBAsync(sede);
        } catch (error) {
            console.error("Error al conectar con la API:", error);
        }
    }

    async addEventoAsync(evento) {
        await this.#putEventoDBAsync(evento);
        try {
            await this.httpService.putAsync(`/eventos`, evento);
            evento.estado = TIPO_EVENTO.Sincronizado;
            await this.#putEventoDBAsync(evento);
        } catch (error) {
            console.error("Error al conectar con la API:", error);
        }
    }

    async refreshEventosAsync() {
        const eventos = await this.#getAllEventosDBAsync();
        const desincronizados = eventos.filter((e) => e.estado !== TIPO_EVENTO.Sincronizado);
        for (const evento of desincronizados) {
            try {
                await this.httpService.putAsync("/eventos", evento);
            } catch (error) {
                console.error("Error al conectar con la API:", error);
            }
        }
    }

    async getAllSedesAsync() {
        try {
            const response = await this.httpService.getAsync(`/sedes`);
            for (const sede of response) {
                sede.estado = TIPO_EVENTO.Sincronizado;
                await this.#putSedeDBAsync(sede);
            }
        } catch (error) {
            console.error("Error al procesar sedes de la API:", error);
        }

        var sedes = this.#getAllSedesDBAsync();
        return sedes;
    }

    async getAllEventosAsync() {
        try {
            const response = await this.httpService.getAsync(`/sedes/${localStorage.getItem(SEDE_KEY)}/eventos`);
            for (const evento of response) {
                evento.estado = TIPO_EVENTO.Sincronizado;
                await this.#putEventoDBAsync(evento);
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
                if (!newDB.objectStoreNames.contains(EVENTOS_STORE_NAME)) {
                    newDB.createObjectStore(EVENTOS_STORE_NAME, {
                        keyPath: "id",
                        autoIncrement: true,
                    });
                }
                if (!newDB.objectStoreNames.contains(SEDES_STORE_NAME)) {
                    newDB.createObjectStore(SEDES_STORE_NAME, {
                        keyPath: "id",
                        autoIncrement: true,
                    });
                }
            };
        });
    }

    async #putSedeDBAsync(sede) {
        const db = await this.#openConnectionAsync();
        const tx = db.transaction([SEDES_STORE_NAME], "readwrite");
        const store = tx.objectStore(SEDES_STORE_NAME);
        return new Promise((resolve, reject) => {
            const request = store.put(sede);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async #putEventoDBAsync(evento) {
        const db = await this.#openConnectionAsync();
        const tx = db.transaction([EVENTOS_STORE_NAME], "readwrite");
        const store = tx.objectStore(EVENTOS_STORE_NAME);
        return new Promise((resolve, reject) => {
            const request = store.put(evento);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async #getAllSedesDBAsync() {
        const db = await this.#openConnectionAsync();
        const tx = db.transaction([SEDES_STORE_NAME], "readonly");
        const store = tx.objectStore(SEDES_STORE_NAME);
        return new Promise((resolve, reject) => {
            const request = store.getAll();
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async #getAllEventosDBAsync() {
        const db = await this.#openConnectionAsync();
        const tx = db.transaction([EVENTOS_STORE_NAME], "readonly");
        const store = tx.objectStore(EVENTOS_STORE_NAME);
        return new Promise((resolve, reject) => {
            const request = store.getAll();
            request.onsuccess = () =>
                resolve(request.result.filter((evento) => evento.sedeId === localStorage.getItem(SEDE_KEY)));
            request.onerror = () => reject(request.error);
        });
    }
}

const repository = new Repository(HTTP_SERVICE);

export default repository;
