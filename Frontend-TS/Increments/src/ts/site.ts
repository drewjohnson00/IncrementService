/**
 * Main application entry point for the Increments Dashboard
 */

import { ApiService } from "./api-service.js";
import { IncrementKey, IncrementCommand } from "./models.js";

// API Service instance (will be initialized on DOMContentLoaded)
let apiService: ApiService;

// Bootstrap Modal instance (will be initialized on DOMContentLoaded)
let addIncrementModal: any;

/**
 * Initialize the application
 */
document.addEventListener("DOMContentLoaded", async () => {
    // Get API base address from data attribute
    const container = document.getElementById("incrementsContainer");
    const configuredAddress = container?.dataset.apiBaseAddress;
    const defaultAddress = "http://localhost:5000";
    const apiBaseAddress = configuredAddress || defaultAddress;

    // Log configuration info
    if (configuredAddress) {
        console.info(`ApiBaseAddress: ${apiBaseAddress}`);
    } else {
        console.warn(`ApiBaseAddress was not found in configuration so default value ${defaultAddress} was used.`);
    }

    // Initialize API Service with configuration
    apiService = new ApiService(apiBaseAddress);

    // Initialize Bootstrap modal
    const modalElement = document.getElementById("addIncrementModal");
    if (modalElement) {
        // @ts-ignore - Bootstrap is loaded via CDN
        addIncrementModal = new bootstrap.Modal(modalElement);
    }

    // Set up event listeners
    setupEventListeners();

    // Load initial data
    await loadIncrements();
});

/**
 * Set up all event listeners
 */
function setupEventListeners(): void {
    // Add increment button
    const addBtn = document.getElementById("addIncrementBtn");
    if (addBtn) {
        addBtn.addEventListener("click", () => {
            clearForm();
            addIncrementModal.show();
        });
    }

    // Form submission
    const form = document.getElementById("addIncrementForm") as HTMLFormElement;
    if (form) {
        form.addEventListener("submit", handleFormSubmit);
    }
}

/**
 * Load all increments from the API
 */
async function loadIncrements(): Promise<void> {
    try {
        const increments = await apiService.getAllIncrements();
        renderIncrements(increments);
    } catch (error) {
        console.error("Failed to load increments:", error);
        showError("Failed to load increments. Please refresh the page.");
    }
}

/**
 * Render all increments to the dashboard
 */
function renderIncrements(increments: IncrementKey[]): void {
    const container = document.getElementById("incrementsContainer");
    if (!container) return;

    // Clear existing content
    container.innerHTML = "";

    // Check if there are no increments
    if (increments.length === 0) {
        container.innerHTML = '<p class="text-muted text-center mt-4">No increments yet. Click the + button to add one!</p>';
        return;
    }

    // Create cards for each increment
    increments.forEach(increment => {
        const card = createIncrementCard(increment);
        container.appendChild(card);
    });
}

/**
 * Create an increment card element
 */
function createIncrementCard(increment: IncrementKey): HTMLElement {
    const card = document.createElement("div");
    card.className = "increment-card";
    card.dataset.key = increment.Key;

    // Format the timestamp
    const lastUsedDate = new Date(increment.LastUsed);
    const formattedDate = lastUsedDate.toString();

    card.innerHTML = `
        <div class="increment-card-header">
            <h3 class="increment-card-title">${escapeHtml(increment.Key)}</h3>
            <button class="increment-card-delete" title="Delete Increment" data-key="${escapeHtml(increment.Key)}">
                <i class="fa-solid fa-trash"></i>
            </button>
        </div>
        <div class="increment-card-body">
            <div class="increment-card-field">
                <p class="increment-card-label">Current Value:</p>
                <p class="increment-card-value large">${increment.PreviousValue}</p>
            </div>
            <div class="increment-card-field">
                <p class="increment-card-timestamp">Last Used on ${formattedDate}</p>
            </div>
            <button class="increment-button" title="Increment" data-key="${escapeHtml(increment.Key)}">
                <i class="fa-solid fa-arrow-up"></i>
            </button>
        </div>
    `;

    // Add delete button event listener
    const deleteBtn = card.querySelector(".increment-card-delete") as HTMLButtonElement;
    if (deleteBtn) {
        deleteBtn.addEventListener("click", () => handleDelete(increment.Key));
    }

    // Add increment button event listener
    const incrementBtn = card.querySelector(".increment-button") as HTMLButtonElement;
    if (incrementBtn) {
        incrementBtn.addEventListener("click", () => handleIncrement(increment.Key, incrementBtn));
    }

    return card;
}

/**
 * Handle form submission for adding a new increment
 */
async function handleFormSubmit(event: Event): Promise<void> {
    event.preventDefault();

    const keyInput = document.getElementById("incrementKey") as HTMLInputElement;
    const valueInput = document.getElementById("incrementPreviousValue") as HTMLInputElement;

    if (!keyInput || !valueInput) return;

    const command: IncrementCommand = {
        Key: keyInput.value.trim(),
        PreviousValue: parseInt(valueInput.value, 10)
    };

    // Validate
    if (command.Key.length < 3 || command.Key.length > 50) {
        showFormError("Key must be between 3 and 50 characters");
        return;
    }

    try {
        // Call API to create increment
        await apiService.upsertIncrement(command);

        // Close modal
        addIncrementModal.hide();

        // Reload increments
        await loadIncrements();
    } catch (error) {
        console.error("Failed to add increment:", error);
        showFormError(error instanceof Error ? error.message : "Failed to add increment");
    }
}

/**
 * Handle delete button click
 */
async function handleDelete(key: string): Promise<void> {
    if (!confirm(`Are you sure you want to delete the increment "${key}"?`)) {
        return;
    }

    try {
        await apiService.deleteIncrement(key);
        await loadIncrements();
    } catch (error) {
        console.error("Failed to delete increment:", error);
        showError(error instanceof Error ? error.message : "Failed to delete increment");
    }
}

/**
 * Handle increment button click
 */
async function handleIncrement(key: string, button: HTMLButtonElement): Promise<void> {
    try {
        // Disable button and keep focus to show blue outline
        button.disabled = true;
        button.focus();

        // Call API to increment the value
        await apiService.incrementValue(key);

        // Reload increments (this will re-enable the button by re-rendering)
        await loadIncrements();
    } catch (error) {
        console.error("Failed to increment value:", error);
        showError(error instanceof Error ? error.message : "Failed to increment value");

        // Re-enable button on error
        button.disabled = false;
    }
}

/**
 * Clear the form and hide error messages
 */
function clearForm(): void {
    const form = document.getElementById("addIncrementForm") as HTMLFormElement;
    if (form) {
        form.reset();
    }

    const errorDiv = document.getElementById("errorMessage");
    if (errorDiv) {
        errorDiv.classList.add("d-none");
        errorDiv.textContent = "";
    }
}

/**
 * Show error message in the form
 */
function showFormError(message: string): void {
    const errorDiv = document.getElementById("errorMessage");
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.classList.remove("d-none");
    }
}

/**
 * Show a general error message (you could enhance this with toast notifications)
 */
function showError(message: string): void {
    alert(message);
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text: string): string {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}
