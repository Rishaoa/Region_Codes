document.addEventListener("DOMContentLoaded", function () {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const mobileQuery = window.matchMedia("(max-width: 767.98px)");

    initializeProgressBars();
    initializeCollectionColors();
    initializeStatsCards();
    initializeOpenedRegionRows();
    initializeNoteButtons();

    function getAntiForgeryToken() {
        return tokenInput ? tokenInput.value : "";
    }

    function isMobile() {
        return mobileQuery.matches;
    }

    function initializeProgressBars() {
        const progressBars = document.querySelectorAll(".progress-bar[data-percent]");

        progressBars.forEach(function (bar) {
            const percent = Number(bar.dataset.percent || 0);
            setProgressBarElement(bar, percent);
        });
    }

    function initializeCollectionColors() {
        const rows = document.querySelectorAll(".region-main-row[data-collection-percent]");

        rows.forEach(function (row) {
            const percent = Number(row.dataset.collectionPercent || 0);
            applyCollectionColor(row, percent);
        });
    }

    function initializeStatsCards() {
        const cards = document.querySelectorAll(".stats-toggle-card");

        cards.forEach(function (card) {
            card.addEventListener("click", function () {
                if (!isMobile()) {
                    return;
                }

                card.classList.toggle("stats-card-open");
            });
        });
    }

    function initializeOpenedRegionRows() {
        const collapses = document.querySelectorAll(".region-collapse");

        collapses.forEach(function (collapse) {
            collapse.addEventListener("shown.bs.collapse", function () {
                setOpenedRegionState(collapse.id, true);
            });

            collapse.addEventListener("hidden.bs.collapse", function () {
                setOpenedRegionState(collapse.id, false);
            });
        });
    }

    function setOpenedRegionState(collapseId, isOpened) {
        const rows = document.querySelectorAll(`[data-bs-target="#${collapseId}"]`);

        rows.forEach(function (row) {
            if (isOpened) {
                row.classList.add("region-row-open");
                row.setAttribute("aria-expanded", "true");
            } else {
                row.classList.remove("region-row-open");
                row.setAttribute("aria-expanded", "false");
            }
        });
    }

    function initializeNoteButtons() {
        const noteInputs = document.querySelectorAll(".note-input");

        noteInputs.forEach(function (input) {
            input.dataset.savedNote = input.value ?? "";

            const button = getSaveButtonForInput(input);

            updateSaveButtonState(input, button);

            input.addEventListener("input", function () {
                updateSaveButtonState(input, button);
            });
        });

        mobileQuery.addEventListener("change", function () {
            refreshAllNoteButtons();
        });
    }

    function getSaveButtonForInput(input) {
        const regionCodeId = input.dataset.regionCodeId;

        return document.querySelector(
            `.save-note-button[data-region-code-id="${regionCodeId}"]`
        );
    }

    function refreshAllNoteButtons() {
        const noteInputs = document.querySelectorAll(".note-input");

        noteInputs.forEach(function (input) {
            updateSaveButtonState(input, getSaveButtonForInput(input));
        });
    }

    function updateSaveButtonState(input, button) {
        if (!button || !input) {
            return;
        }

        const currentValue = input.value ?? "";
        const savedValue = input.dataset.savedNote ?? "";
        const hasChanged = currentValue !== savedValue;
        const canEdit = !input.disabled;

        button.style.display = canEdit && hasChanged ? "" : "none";
        button.disabled = !canEdit || !hasChanged;
    }

    function applyCollectionColor(row, percent) {
        const safePercent = Math.max(0, Math.min(100, Number(percent) || 0));

        row.dataset.collectionPercent = safePercent;

        if (safePercent <= 0) {
            row.style.setProperty("--collection-bg", "#ffffff");
            row.classList.add("collection-empty");
            return;
        }

        if (safePercent >= 100) {
            row.style.setProperty("--collection-bg", "#e6dcff");
            row.classList.remove("collection-empty");
            return;
        }

        const startColor = {
            r: 239,
            g: 251,
            b: 255
        };

        const endColor = {
            r: 197,
            g: 238,
            b: 248
        };

        const ratio = safePercent / 99;

        const red = Math.round(startColor.r + (endColor.r - startColor.r) * ratio);
        const green = Math.round(startColor.g + (endColor.g - startColor.g) * ratio);
        const blue = Math.round(startColor.b + (endColor.b - startColor.b) * ratio);

        const resultColor = `rgb(${red}, ${green}, ${blue})`;

        row.style.setProperty("--collection-bg", resultColor);
        row.classList.remove("collection-empty");
    }

    function setProgressBarElement(bar, percent) {
        const safePercent = Math.max(0, Math.min(100, Number(percent) || 0));

        bar.style.width = `${safePercent}%`;
        bar.setAttribute("aria-valuenow", safePercent);

        const progress = bar.closest(".progress");
        const percentText = progress
            ? progress.querySelector(".progress-percent")
            : null;

        if (progress) {
            if (safePercent >= 50) {
                progress.classList.add("progress-text-light");
            } else {
                progress.classList.remove("progress-text-light");
            }
        }

        if (percentText) {
            percentText.textContent = `${safePercent}%`;
        } else {
            bar.textContent = `${safePercent}%`;
        }
    }

    function formatDateHtml(dateText) {
        if (!dateText) {
            return "—";
        }

        const parts = dateText.split(" ");

        if (parts.length < 2) {
            return `<span class="date-main">${dateText}</span>`;
        }

        return `<span class="date-main">${parts[0]}</span><span class="date-time">${parts[1]}</span>`;
    }

    async function postToHandler(handlerName, data) {
        const body = new URLSearchParams();

        for (const key in data) {
            body.append(key, data[key] ?? "");
        }

        body.append("__RequestVerificationToken", getAntiForgeryToken());

        const response = await fetch(`/MyRegions?handler=${handlerName}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: body.toString()
        });

        if (response.status === 401) {
            window.location.href = "/Account/Login";
            return null;
        }

        if (!response.ok) {
            throw new Error("Ошибка запроса");
        }

        return await response.json();
    }

    function setProgressBar(barId, percent) {
        const bar = document.getElementById(barId);

        if (!bar) {
            return;
        }

        setProgressBarElement(bar, percent);
    }

    function updateProgress(progress) {
        document.getElementById("seenCodesText").textContent =
            `${progress.seenCodes} из ${progress.totalCodes}`;

        document.getElementById("seenRegionsText").textContent =
            `${progress.seenRegions} из ${progress.totalRegions}`;

        document.getElementById("completedRegionsText").textContent =
            `${progress.completedRegions} из ${progress.totalRegions}`;

        setProgressBar("codeProgressBar", progress.codeProgressPercent);
        setProgressBar("regionProgressBar", progress.regionProgressPercent);
        setProgressBar("completedProgressBar", progress.completedProgressPercent);
    }

    function updateRegion(region) {
        const rows = document.querySelectorAll(
            `.region-main-row[data-region-id="${region.regionId}"]`
        );

        rows.forEach(function (row) {
            applyCollectionColor(row, region.collectionPercent);
        });
    }

    function updateCodes(codes) {
        codes.forEach(function (code) {
            const checkboxes = document.querySelectorAll(
                `.code-checkbox[data-region-code-id="${code.regionCodeId}"]`
            );

            checkboxes.forEach(function (checkbox) {
                checkbox.checked = code.isSeen;
                checkbox.disabled = false;
            });

            const dateElements = document.querySelectorAll(
                `.code-date[data-region-code-id="${code.regionCodeId}"]`
            );

            dateElements.forEach(function (dateElement) {
                if (code.seenAt) {
                    dateElement.innerHTML = formatDateHtml(code.seenAt);
                    dateElement.classList.remove("text-muted");
                } else {
                    dateElement.textContent = "—";
                    dateElement.classList.add("text-muted");
                }
            });

            const noteInputs = document.querySelectorAll(
                `.note-input[data-region-code-id="${code.regionCodeId}"]`
            );

            noteInputs.forEach(function (input) {
                const newNote = code.note ?? "";

                input.disabled = !code.isSeen;
                input.value = newNote;
                input.dataset.savedNote = newNote;

                const button = getSaveButtonForInput(input);
                updateSaveButtonState(input, button);
            });
        });

        refreshAllNoteButtons();
    }

    document.querySelectorAll(".code-checkbox").forEach(function (checkbox) {
        checkbox.addEventListener("change", async function () {
            const regionCodeId = this.dataset.regionCodeId;
            const newCheckedState = this.checked;

            const relatedCheckboxes = document.querySelectorAll(
                `.code-checkbox[data-region-code-id="${regionCodeId}"]`
            );

            relatedCheckboxes.forEach(function (item) {
                item.checked = newCheckedState;
                item.disabled = true;
            });

            try {
                const result = await postToHandler("ToggleCodeAjax", {
                    regionCodeId: regionCodeId
                });

                if (!result || !result.success) {
                    throw new Error("Не удалось сохранить отметку");
                }

                updateProgress(result.progress);
                updateRegion(result.region);
                updateCodes(result.codes);
            } catch {
                relatedCheckboxes.forEach(function (item) {
                    item.checked = !newCheckedState;
                    item.disabled = false;
                });

                alert("Не удалось сохранить отметку. Попробуйте ещё раз.");
            }
        });
    });

    document.querySelectorAll(".save-note-button").forEach(function (button) {
        button.addEventListener("click", async function () {
            const regionCodeId = this.dataset.regionCodeId;

            const input = document.querySelector(
                `.note-input[data-region-code-id="${regionCodeId}"]`
            );

            if (!input || this.disabled) {
                return;
            }

            const oldText = this.textContent.trim();

            this.disabled = true;
            this.textContent = "Сохранение...";

            try {
                const result = await postToHandler("SaveNoteAjax", {
                    regionCodeId: regionCodeId,
                    note: input.value
                });

                if (!result || !result.success) {
                    throw new Error("Не удалось сохранить заметку");
                }

                const savedNote = result.note ?? "";

                const noteInputs = document.querySelectorAll(
                    `.note-input[data-region-code-id="${regionCodeId}"]`
                );

                noteInputs.forEach(function (noteInput) {
                    noteInput.value = savedNote;
                    noteInput.dataset.savedNote = savedNote;
                });

                this.textContent = "Сохранено";

                setTimeout(() => {
                    this.textContent = oldText || "Сохранить";

                    noteInputs.forEach(function (noteInput) {
                        const relatedButton = getSaveButtonForInput(noteInput);
                        updateSaveButtonState(noteInput, relatedButton);
                    });

                    refreshAllNoteButtons();
                }, 900);
            } catch {
                alert("Не удалось сохранить заметку. Попробуйте ещё раз.");

                this.textContent = oldText || "Сохранить";
                this.disabled = false;
                refreshAllNoteButtons();
            }
        });
    });
});