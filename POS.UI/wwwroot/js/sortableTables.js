;(function () {
    "use strict";

    const collator = new Intl.Collator(undefined, { numeric: true, sensitivity: "base" });

    const normalize = (value, type) => {
        if (value == null)
            return "";

        const raw = value.toString().trim();

        if (type === "number") {
            const numeric = raw.replace(/[^\d.-]/g, "");
            const parsed = parseFloat(numeric);
            return Number.isFinite(parsed) ? parsed : 0;
        }

        if (type === "date") {
            const parsed = Date.parse(raw);
            return Number.isFinite(parsed) ? parsed : 0;
        }

        return raw.toLowerCase();
    };

    const getCellValue = (row, index) => {
        const cell = row.cells[index];
        if (!cell)
            return "";

        if (cell.dataset.sortValue !== undefined) {
            return cell.dataset.sortValue;
        }

        return cell.textContent ?? "";
    };

    const sortRows = (rows, index, direction, type) => {
        return rows.sort((a, b) => {
            const valueA = normalize(getCellValue(a, index), type);
            const valueB = normalize(getCellValue(b, index), type);

            if (typeof valueA === "number" && typeof valueB === "number") {
                return direction * (valueA - valueB);
            }

            if (typeof valueA === "number" && typeof valueB === "string") {
                return direction * 1;
            }

            if (typeof valueA === "string" && typeof valueB === "number") {
                return direction * -1;
            }

            return direction * collator.compare(valueA, valueB);
        });
    };

    const handleHeaderClick = (event) => {
        const header = event.target.closest("th");
        if (!header)
            return;

        const table = header.closest("table.sortable-table");
        if (!table)
            return;

        const thead = table.tHead;
        if (!thead)
            return;

        const row = header.closest("tr");
        if (!row)
            return;

        const headers = Array.from(row.cells);
        const columnIndex = headers.indexOf(header);
        if (columnIndex < 0)
            return;

        if (header.dataset.sortable === "false")
            return;

        const tbody = table.tBodies[0];
        if (!tbody)
            return;

        const currentDirection = header.dataset.sortDirection === "asc" ? "desc" : "asc";
        headers.forEach(h => {
            delete h.dataset.sortDirection;
            h.removeAttribute("aria-sort");
        });

        header.dataset.sortDirection = currentDirection;
        header.setAttribute("aria-sort", currentDirection === "asc" ? "ascending" : "descending");

        const rows = Array.from(tbody.rows);
        const direction = currentDirection === "asc" ? 1 : -1;
        const type = header.dataset.sortType ?? "text";

        const sorted = sortRows(rows, columnIndex, direction, type);

        const fragment = document.createDocumentFragment();
        sorted.forEach(r => fragment.appendChild(r));
        tbody.appendChild(fragment);
    };

    document.addEventListener("click", handleHeaderClick);

    window.SortableTables = {
        refresh() {
            // Intentionally left empty. Tables sort on the fly via delegated handler.
        }
    };
})();
