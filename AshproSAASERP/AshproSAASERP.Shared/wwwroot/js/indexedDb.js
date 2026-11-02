const dbName = "erpdb";
const dbVersion = 4; // ⬅ bump version

const STORES = [
    "menus",
    "erpdetails"
];

// Open DB
function openDb() {

    return new Promise((resolve, reject) => {

        const req = indexedDB.open(dbName, dbVersion);

        req.onupgradeneeded = e => {

            const db = e.target.result;

            STORES.forEach(store => {

                if (!db.objectStoreNames.contains(store)) {
                    db.createObjectStore(store, {
                        keyPath: "id",
                        autoIncrement: false
                    });
                }

            });
        };

        req.onsuccess = e => resolve(e.target.result);
        req.onerror = () => reject(req.error);
    });
}

// SAVE / UPDATE
export async function upsert(store, data) {

    const db = await openDb();

    if (!db.objectStoreNames.contains(store))
        throw `Store '${store}' not found`;

    return new Promise((resolve, reject) => {

        const tx = db.transaction(store, "readwrite");
        const req = tx.objectStore(store).put(data);

        req.onsuccess = () => resolve();
        req.onerror = () => reject(req.error);
        tx.onabort = () => reject(tx.error);
    });
}

// GET ALL
export async function getAll(store) {

    const db = await openDb();

    return new Promise((resolve, reject) => {

        const tx = db.transaction(store, "readonly");
        const req = tx.objectStore(store).getAll();

        req.onsuccess = () => resolve(req.result ?? []);
        req.onerror = () => reject(req.error);
    });
}

// GET BY ID
export async function getById(store, id) {

    const db = await openDb();

    return new Promise((resolve, reject) => {

        const tx = db.transaction(store, "readonly");
        const req = tx.objectStore(store).get(id);

        req.onsuccess = () => resolve(req.result ?? null);
        req.onerror = () => reject(req.error);
    });
}

// DELETE
export async function remove(store, id) {

    const db = await openDb();

    return new Promise((resolve, reject) => {

        const tx = db.transaction(store, "readwrite");
        const req = tx.objectStore(store).delete(id);

        req.onsuccess = () => resolve();
        req.onerror = () => reject(req.error);
    });
}

// CLEAR
export async function clear(store) {

    const db = await openDb();

    return new Promise((resolve, reject) => {

        const tx = db.transaction(store, "readwrite");
        const req = tx.objectStore(store).clear();

        req.onsuccess = () => resolve();
        req.onerror = () => reject(req.error);
    });
}
