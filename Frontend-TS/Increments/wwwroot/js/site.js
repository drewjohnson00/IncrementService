/**
 * Main application entry point for the Increments Dashboard
 */
import { ApiService } from "./api-service.js";
// API Service instance (will be initialized on DOMContentLoaded)
let apiService;
// Bootstrap Modal instance (will be initialized on DOMContentLoaded)
let addIncrementModal;
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
    }
    else {
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
function setupEventListeners() {
    // Add increment button
    const addBtn = document.getElementById("addIncrementBtn");
    if (addBtn) {
        addBtn.addEventListener("click", () => {
            clearForm();
            addIncrementModal.show();
        });
    }
    // Form submission
    const form = document.getElementById("addIncrementForm");
    if (form) {
        form.addEventListener("submit", handleFormSubmit);
    }
}
/**
 * Load all increments from the API
 */
async function loadIncrements() {
    try {
        const increments = await apiService.getAllIncrements();
        renderIncrements(increments);
    }
    catch (error) {
        console.error("Failed to load increments:", error);
        showError("Failed to load increments. Please refresh the page.");
    }
}
/**
 * Render all increments to the dashboard
 */
function renderIncrements(increments) {
    const container = document.getElementById("incrementsContainer");
    if (!container)
        return;
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
function createIncrementCard(increment) {
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
    const deleteBtn = card.querySelector(".increment-card-delete");
    if (deleteBtn) {
        deleteBtn.addEventListener("click", () => handleDelete(increment.Key));
    }
    // Add increment button event listener
    const incrementBtn = card.querySelector(".increment-button");
    if (incrementBtn) {
        incrementBtn.addEventListener("click", () => handleIncrement(increment.Key, incrementBtn));
    }
    return card;
}
/**
 * Handle form submission for adding a new increment
 */
async function handleFormSubmit(event) {
    event.preventDefault();
    const keyInput = document.getElementById("incrementKey");
    const valueInput = document.getElementById("incrementPreviousValue");
    if (!keyInput || !valueInput)
        return;
    const command = {
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
    }
    catch (error) {
        console.error("Failed to add increment:", error);
        showFormError(error instanceof Error ? error.message : "Failed to add increment");
    }
}
/**
 * Handle delete button click
 */
async function handleDelete(key) {
    if (!confirm(`Are you sure you want to delete the increment "${key}"?`)) {
        return;
    }
    try {
        await apiService.deleteIncrement(key);
        await loadIncrements();
    }
    catch (error) {
        console.error("Failed to delete increment:", error);
        showError(error instanceof Error ? error.message : "Failed to delete increment");
    }
}
/**
 * Handle increment button click
 */
async function handleIncrement(key, button) {
    try {
        // Disable button and keep focus to show blue outline
        button.disabled = true;
        button.focus();
        // Call API to increment the value
        await apiService.incrementValue(key);
        // Reload increments (this will re-enable the button by re-rendering)
        await loadIncrements();
    }
    catch (error) {
        console.error("Failed to increment value:", error);
        showError(error instanceof Error ? error.message : "Failed to increment value");
        // Re-enable button on error
        button.disabled = false;
    }
}
/**
 * Clear the form and hide error messages
 */
function clearForm() {
    const form = document.getElementById("addIncrementForm");
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
function showFormError(message) {
    const errorDiv = document.getElementById("errorMessage");
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.classList.remove("d-none");
    }
}
/**
 * Show a general error message (you could enhance this with toast notifications)
 */
function showError(message) {
    alert(message);
}
/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoic2l0ZS5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzIjpbIi4uLy4uL3NyYy90cy9zaXRlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBOztHQUVHO0FBRUgsT0FBTyxFQUFFLFVBQVUsRUFBRSxNQUFNLGtCQUFrQixDQUFDO0FBRzlDLGlFQUFpRTtBQUNqRSxJQUFJLFVBQXNCLENBQUM7QUFFM0IscUVBQXFFO0FBQ3JFLElBQUksaUJBQXNCLENBQUM7QUFFM0I7O0dBRUc7QUFDSCxRQUFRLENBQUMsZ0JBQWdCLENBQUMsa0JBQWtCLEVBQUUsS0FBSyxJQUFJLEVBQUU7SUFDckQsMkNBQTJDO0lBQzNDLE1BQU0sU0FBUyxHQUFHLFFBQVEsQ0FBQyxjQUFjLENBQUMscUJBQXFCLENBQUMsQ0FBQztJQUNqRSxNQUFNLGlCQUFpQixHQUFHLFNBQVMsRUFBRSxPQUFPLENBQUMsY0FBYyxDQUFDO0lBQzVELE1BQU0sY0FBYyxHQUFHLHVCQUF1QixDQUFDO0lBQy9DLE1BQU0sY0FBYyxHQUFHLGlCQUFpQixJQUFJLGNBQWMsQ0FBQztJQUUzRCx5QkFBeUI7SUFDekIsSUFBSSxpQkFBaUIsRUFBRSxDQUFDO1FBQ3BCLE9BQU8sQ0FBQyxJQUFJLENBQUMsbUJBQW1CLGNBQWMsRUFBRSxDQUFDLENBQUM7SUFDdEQsQ0FBQztTQUFNLENBQUM7UUFDSixPQUFPLENBQUMsSUFBSSxDQUFDLGtFQUFrRSxjQUFjLFlBQVksQ0FBQyxDQUFDO0lBQy9HLENBQUM7SUFFRCw0Q0FBNEM7SUFDNUMsVUFBVSxHQUFHLElBQUksVUFBVSxDQUFDLGNBQWMsQ0FBQyxDQUFDO0lBRTVDLDZCQUE2QjtJQUM3QixNQUFNLFlBQVksR0FBRyxRQUFRLENBQUMsY0FBYyxDQUFDLG1CQUFtQixDQUFDLENBQUM7SUFDbEUsSUFBSSxZQUFZLEVBQUUsQ0FBQztRQUNmLDJDQUEyQztRQUMzQyxpQkFBaUIsR0FBRyxJQUFJLFNBQVMsQ0FBQyxLQUFLLENBQUMsWUFBWSxDQUFDLENBQUM7SUFDMUQsQ0FBQztJQUVELHlCQUF5QjtJQUN6QixtQkFBbUIsRUFBRSxDQUFDO0lBRXRCLG9CQUFvQjtJQUNwQixNQUFNLGNBQWMsRUFBRSxDQUFDO0FBQzNCLENBQUMsQ0FBQyxDQUFDO0FBRUg7O0dBRUc7QUFDSCxTQUFTLG1CQUFtQjtJQUN4Qix1QkFBdUI7SUFDdkIsTUFBTSxNQUFNLEdBQUcsUUFBUSxDQUFDLGNBQWMsQ0FBQyxpQkFBaUIsQ0FBQyxDQUFDO0lBQzFELElBQUksTUFBTSxFQUFFLENBQUM7UUFDVCxNQUFNLENBQUMsZ0JBQWdCLENBQUMsT0FBTyxFQUFFLEdBQUcsRUFBRTtZQUNsQyxTQUFTLEVBQUUsQ0FBQztZQUNaLGlCQUFpQixDQUFDLElBQUksRUFBRSxDQUFDO1FBQzdCLENBQUMsQ0FBQyxDQUFDO0lBQ1AsQ0FBQztJQUVELGtCQUFrQjtJQUNsQixNQUFNLElBQUksR0FBRyxRQUFRLENBQUMsY0FBYyxDQUFDLGtCQUFrQixDQUFvQixDQUFDO0lBQzVFLElBQUksSUFBSSxFQUFFLENBQUM7UUFDUCxJQUFJLENBQUMsZ0JBQWdCLENBQUMsUUFBUSxFQUFFLGdCQUFnQixDQUFDLENBQUM7SUFDdEQsQ0FBQztBQUNMLENBQUM7QUFFRDs7R0FFRztBQUNILEtBQUssVUFBVSxjQUFjO0lBQ3pCLElBQUksQ0FBQztRQUNELE1BQU0sVUFBVSxHQUFHLE1BQU0sVUFBVSxDQUFDLGdCQUFnQixFQUFFLENBQUM7UUFDdkQsZ0JBQWdCLENBQUMsVUFBVSxDQUFDLENBQUM7SUFDakMsQ0FBQztJQUFDLE9BQU8sS0FBSyxFQUFFLENBQUM7UUFDYixPQUFPLENBQUMsS0FBSyxDQUFDLDRCQUE0QixFQUFFLEtBQUssQ0FBQyxDQUFDO1FBQ25ELFNBQVMsQ0FBQyxxREFBcUQsQ0FBQyxDQUFDO0lBQ3JFLENBQUM7QUFDTCxDQUFDO0FBRUQ7O0dBRUc7QUFDSCxTQUFTLGdCQUFnQixDQUFDLFVBQTBCO0lBQ2hELE1BQU0sU0FBUyxHQUFHLFFBQVEsQ0FBQyxjQUFjLENBQUMscUJBQXFCLENBQUMsQ0FBQztJQUNqRSxJQUFJLENBQUMsU0FBUztRQUFFLE9BQU87SUFFdkIseUJBQXlCO0lBQ3pCLFNBQVMsQ0FBQyxTQUFTLEdBQUcsRUFBRSxDQUFDO0lBRXpCLG1DQUFtQztJQUNuQyxJQUFJLFVBQVUsQ0FBQyxNQUFNLEtBQUssQ0FBQyxFQUFFLENBQUM7UUFDMUIsU0FBUyxDQUFDLFNBQVMsR0FBRyw4RkFBOEYsQ0FBQztRQUNySCxPQUFPO0lBQ1gsQ0FBQztJQUVELGtDQUFrQztJQUNsQyxVQUFVLENBQUMsT0FBTyxDQUFDLFNBQVMsQ0FBQyxFQUFFO1FBQzNCLE1BQU0sSUFBSSxHQUFHLG1CQUFtQixDQUFDLFNBQVMsQ0FBQyxDQUFDO1FBQzVDLFNBQVMsQ0FBQyxXQUFXLENBQUMsSUFBSSxDQUFDLENBQUM7SUFDaEMsQ0FBQyxDQUFDLENBQUM7QUFDUCxDQUFDO0FBRUQ7O0dBRUc7QUFDSCxTQUFTLG1CQUFtQixDQUFDLFNBQXVCO0lBQ2hELE1BQU0sSUFBSSxHQUFHLFFBQVEsQ0FBQyxhQUFhLENBQUMsS0FBSyxDQUFDLENBQUM7SUFDM0MsSUFBSSxDQUFDLFNBQVMsR0FBRyxnQkFBZ0IsQ0FBQztJQUNsQyxJQUFJLENBQUMsT0FBTyxDQUFDLEdBQUcsR0FBRyxTQUFTLENBQUMsR0FBRyxDQUFDO0lBRWpDLHVCQUF1QjtJQUN2QixNQUFNLFlBQVksR0FBRyxJQUFJLElBQUksQ0FBQyxTQUFTLENBQUMsUUFBUSxDQUFDLENBQUM7SUFDbEQsTUFBTSxhQUFhLEdBQUcsWUFBWSxDQUFDLFFBQVEsRUFBRSxDQUFDO0lBRTlDLElBQUksQ0FBQyxTQUFTLEdBQUc7OytDQUUwQixVQUFVLENBQUMsU0FBUyxDQUFDLEdBQUcsQ0FBQzt1RkFDZSxVQUFVLENBQUMsU0FBUyxDQUFDLEdBQUcsQ0FBQzs7Ozs7Ozt3REFPeEQsU0FBUyxDQUFDLGFBQWE7OzttRUFHWixhQUFhOzsyRUFFTCxVQUFVLENBQUMsU0FBUyxDQUFDLEdBQUcsQ0FBQzs7OztLQUkvRixDQUFDO0lBRUYsbUNBQW1DO0lBQ25DLE1BQU0sU0FBUyxHQUFHLElBQUksQ0FBQyxhQUFhLENBQUMsd0JBQXdCLENBQXNCLENBQUM7SUFDcEYsSUFBSSxTQUFTLEVBQUUsQ0FBQztRQUNaLFNBQVMsQ0FBQyxnQkFBZ0IsQ0FBQyxPQUFPLEVBQUUsR0FBRyxFQUFFLENBQUMsWUFBWSxDQUFDLFNBQVMsQ0FBQyxHQUFHLENBQUMsQ0FBQyxDQUFDO0lBQzNFLENBQUM7SUFFRCxzQ0FBc0M7SUFDdEMsTUFBTSxZQUFZLEdBQUcsSUFBSSxDQUFDLGFBQWEsQ0FBQyxtQkFBbUIsQ0FBc0IsQ0FBQztJQUNsRixJQUFJLFlBQVksRUFBRSxDQUFDO1FBQ2YsWUFBWSxDQUFDLGdCQUFnQixDQUFDLE9BQU8sRUFBRSxHQUFHLEVBQUUsQ0FBQyxlQUFlLENBQUMsU0FBUyxDQUFDLEdBQUcsRUFBRSxZQUFZLENBQUMsQ0FBQyxDQUFDO0lBQy9GLENBQUM7SUFFRCxPQUFPLElBQUksQ0FBQztBQUNoQixDQUFDO0FBRUQ7O0dBRUc7QUFDSCxLQUFLLFVBQVUsZ0JBQWdCLENBQUMsS0FBWTtJQUN4QyxLQUFLLENBQUMsY0FBYyxFQUFFLENBQUM7SUFFdkIsTUFBTSxRQUFRLEdBQUcsUUFBUSxDQUFDLGNBQWMsQ0FBQyxjQUFjLENBQXFCLENBQUM7SUFDN0UsTUFBTSxVQUFVLEdBQUcsUUFBUSxDQUFDLGNBQWMsQ0FBQyx3QkFBd0IsQ0FBcUIsQ0FBQztJQUV6RixJQUFJLENBQUMsUUFBUSxJQUFJLENBQUMsVUFBVTtRQUFFLE9BQU87SUFFckMsTUFBTSxPQUFPLEdBQXFCO1FBQzlCLEdBQUcsRUFBRSxRQUFRLENBQUMsS0FBSyxDQUFDLElBQUksRUFBRTtRQUMxQixhQUFhLEVBQUUsUUFBUSxDQUFDLFVBQVUsQ0FBQyxLQUFLLEVBQUUsRUFBRSxDQUFDO0tBQ2hELENBQUM7SUFFRixXQUFXO0lBQ1gsSUFBSSxPQUFPLENBQUMsR0FBRyxDQUFDLE1BQU0sR0FBRyxDQUFDLElBQUksT0FBTyxDQUFDLEdBQUcsQ0FBQyxNQUFNLEdBQUcsRUFBRSxFQUFFLENBQUM7UUFDcEQsYUFBYSxDQUFDLHlDQUF5QyxDQUFDLENBQUM7UUFDekQsT0FBTztJQUNYLENBQUM7SUFFRCxJQUFJLENBQUM7UUFDRCwrQkFBK0I7UUFDL0IsTUFBTSxVQUFVLENBQUMsZUFBZSxDQUFDLE9BQU8sQ0FBQyxDQUFDO1FBRTFDLGNBQWM7UUFDZCxpQkFBaUIsQ0FBQyxJQUFJLEVBQUUsQ0FBQztRQUV6QixvQkFBb0I7UUFDcEIsTUFBTSxjQUFjLEVBQUUsQ0FBQztJQUMzQixDQUFDO0lBQUMsT0FBTyxLQUFLLEVBQUUsQ0FBQztRQUNiLE9BQU8sQ0FBQyxLQUFLLENBQUMsMEJBQTBCLEVBQUUsS0FBSyxDQUFDLENBQUM7UUFDakQsYUFBYSxDQUFDLEtBQUssWUFBWSxLQUFLLENBQUMsQ0FBQyxDQUFDLEtBQUssQ0FBQyxPQUFPLENBQUMsQ0FBQyxDQUFDLHlCQUF5QixDQUFDLENBQUM7SUFDdEYsQ0FBQztBQUNMLENBQUM7QUFFRDs7R0FFRztBQUNILEtBQUssVUFBVSxZQUFZLENBQUMsR0FBVztJQUNuQyxJQUFJLENBQUMsT0FBTyxDQUFDLGtEQUFrRCxHQUFHLElBQUksQ0FBQyxFQUFFLENBQUM7UUFDdEUsT0FBTztJQUNYLENBQUM7SUFFRCxJQUFJLENBQUM7UUFDRCxNQUFNLFVBQVUsQ0FBQyxlQUFlLENBQUMsR0FBRyxDQUFDLENBQUM7UUFDdEMsTUFBTSxjQUFjLEVBQUUsQ0FBQztJQUMzQixDQUFDO0lBQUMsT0FBTyxLQUFLLEVBQUUsQ0FBQztRQUNiLE9BQU8sQ0FBQyxLQUFLLENBQUMsNkJBQTZCLEVBQUUsS0FBSyxDQUFDLENBQUM7UUFDcEQsU0FBUyxDQUFDLEtBQUssWUFBWSxLQUFLLENBQUMsQ0FBQyxDQUFDLEtBQUssQ0FBQyxPQUFPLENBQUMsQ0FBQyxDQUFDLDRCQUE0QixDQUFDLENBQUM7SUFDckYsQ0FBQztBQUNMLENBQUM7QUFFRDs7R0FFRztBQUNILEtBQUssVUFBVSxlQUFlLENBQUMsR0FBVyxFQUFFLE1BQXlCO0lBQ2pFLElBQUksQ0FBQztRQUNELHFEQUFxRDtRQUNyRCxNQUFNLENBQUMsUUFBUSxHQUFHLElBQUksQ0FBQztRQUN2QixNQUFNLENBQUMsS0FBSyxFQUFFLENBQUM7UUFFZixrQ0FBa0M7UUFDbEMsTUFBTSxVQUFVLENBQUMsY0FBYyxDQUFDLEdBQUcsQ0FBQyxDQUFDO1FBRXJDLHFFQUFxRTtRQUNyRSxNQUFNLGNBQWMsRUFBRSxDQUFDO0lBQzNCLENBQUM7SUFBQyxPQUFPLEtBQUssRUFBRSxDQUFDO1FBQ2IsT0FBTyxDQUFDLEtBQUssQ0FBQyw0QkFBNEIsRUFBRSxLQUFLLENBQUMsQ0FBQztRQUNuRCxTQUFTLENBQUMsS0FBSyxZQUFZLEtBQUssQ0FBQyxDQUFDLENBQUMsS0FBSyxDQUFDLE9BQU8sQ0FBQyxDQUFDLENBQUMsMkJBQTJCLENBQUMsQ0FBQztRQUVoRiw0QkFBNEI7UUFDNUIsTUFBTSxDQUFDLFFBQVEsR0FBRyxLQUFLLENBQUM7SUFDNUIsQ0FBQztBQUNMLENBQUM7QUFFRDs7R0FFRztBQUNILFNBQVMsU0FBUztJQUNkLE1BQU0sSUFBSSxHQUFHLFFBQVEsQ0FBQyxjQUFjLENBQUMsa0JBQWtCLENBQW9CLENBQUM7SUFDNUUsSUFBSSxJQUFJLEVBQUUsQ0FBQztRQUNQLElBQUksQ0FBQyxLQUFLLEVBQUUsQ0FBQztJQUNqQixDQUFDO0lBRUQsTUFBTSxRQUFRLEdBQUcsUUFBUSxDQUFDLGNBQWMsQ0FBQyxjQUFjLENBQUMsQ0FBQztJQUN6RCxJQUFJLFFBQVEsRUFBRSxDQUFDO1FBQ1gsUUFBUSxDQUFDLFNBQVMsQ0FBQyxHQUFHLENBQUMsUUFBUSxDQUFDLENBQUM7UUFDakMsUUFBUSxDQUFDLFdBQVcsR0FBRyxFQUFFLENBQUM7SUFDOUIsQ0FBQztBQUNMLENBQUM7QUFFRDs7R0FFRztBQUNILFNBQVMsYUFBYSxDQUFDLE9BQWU7SUFDbEMsTUFBTSxRQUFRLEdBQUcsUUFBUSxDQUFDLGNBQWMsQ0FBQyxjQUFjLENBQUMsQ0FBQztJQUN6RCxJQUFJLFFBQVEsRUFBRSxDQUFDO1FBQ1gsUUFBUSxDQUFDLFdBQVcsR0FBRyxPQUFPLENBQUM7UUFDL0IsUUFBUSxDQUFDLFNBQVMsQ0FBQyxNQUFNLENBQUMsUUFBUSxDQUFDLENBQUM7SUFDeEMsQ0FBQztBQUNMLENBQUM7QUFFRDs7R0FFRztBQUNILFNBQVMsU0FBUyxDQUFDLE9BQWU7SUFDOUIsS0FBSyxDQUFDLE9BQU8sQ0FBQyxDQUFDO0FBQ25CLENBQUM7QUFFRDs7R0FFRztBQUNILFNBQVMsVUFBVSxDQUFDLElBQVk7SUFDNUIsTUFBTSxHQUFHLEdBQUcsUUFBUSxDQUFDLGFBQWEsQ0FBQyxLQUFLLENBQUMsQ0FBQztJQUMxQyxHQUFHLENBQUMsV0FBVyxHQUFHLElBQUksQ0FBQztJQUN2QixPQUFPLEdBQUcsQ0FBQyxTQUFTLENBQUM7QUFDekIsQ0FBQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxyXG4gKiBNYWluIGFwcGxpY2F0aW9uIGVudHJ5IHBvaW50IGZvciB0aGUgSW5jcmVtZW50cyBEYXNoYm9hcmRcclxuICovXHJcblxyXG5pbXBvcnQgeyBBcGlTZXJ2aWNlIH0gZnJvbSBcIi4vYXBpLXNlcnZpY2UuanNcIjtcclxuaW1wb3J0IHsgSW5jcmVtZW50S2V5LCBJbmNyZW1lbnRDb21tYW5kIH0gZnJvbSBcIi4vbW9kZWxzLmpzXCI7XHJcblxyXG4vLyBBUEkgU2VydmljZSBpbnN0YW5jZSAod2lsbCBiZSBpbml0aWFsaXplZCBvbiBET01Db250ZW50TG9hZGVkKVxyXG5sZXQgYXBpU2VydmljZTogQXBpU2VydmljZTtcclxuXHJcbi8vIEJvb3RzdHJhcCBNb2RhbCBpbnN0YW5jZSAod2lsbCBiZSBpbml0aWFsaXplZCBvbiBET01Db250ZW50TG9hZGVkKVxyXG5sZXQgYWRkSW5jcmVtZW50TW9kYWw6IGFueTtcclxuXHJcbi8qKlxyXG4gKiBJbml0aWFsaXplIHRoZSBhcHBsaWNhdGlvblxyXG4gKi9cclxuZG9jdW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcIkRPTUNvbnRlbnRMb2FkZWRcIiwgYXN5bmMgKCkgPT4ge1xyXG4gICAgLy8gR2V0IEFQSSBiYXNlIGFkZHJlc3MgZnJvbSBkYXRhIGF0dHJpYnV0ZVxyXG4gICAgY29uc3QgY29udGFpbmVyID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoXCJpbmNyZW1lbnRzQ29udGFpbmVyXCIpO1xyXG4gICAgY29uc3QgY29uZmlndXJlZEFkZHJlc3MgPSBjb250YWluZXI/LmRhdGFzZXQuYXBpQmFzZUFkZHJlc3M7XHJcbiAgICBjb25zdCBkZWZhdWx0QWRkcmVzcyA9IFwiaHR0cDovL2xvY2FsaG9zdDo1MDAwXCI7XHJcbiAgICBjb25zdCBhcGlCYXNlQWRkcmVzcyA9IGNvbmZpZ3VyZWRBZGRyZXNzIHx8IGRlZmF1bHRBZGRyZXNzO1xyXG5cclxuICAgIC8vIExvZyBjb25maWd1cmF0aW9uIGluZm9cclxuICAgIGlmIChjb25maWd1cmVkQWRkcmVzcykge1xyXG4gICAgICAgIGNvbnNvbGUuaW5mbyhgQXBpQmFzZUFkZHJlc3M6ICR7YXBpQmFzZUFkZHJlc3N9YCk7XHJcbiAgICB9IGVsc2Uge1xyXG4gICAgICAgIGNvbnNvbGUud2FybihgQXBpQmFzZUFkZHJlc3Mgd2FzIG5vdCBmb3VuZCBpbiBjb25maWd1cmF0aW9uIHNvIGRlZmF1bHQgdmFsdWUgJHtkZWZhdWx0QWRkcmVzc30gd2FzIHVzZWQuYCk7XHJcbiAgICB9XHJcblxyXG4gICAgLy8gSW5pdGlhbGl6ZSBBUEkgU2VydmljZSB3aXRoIGNvbmZpZ3VyYXRpb25cclxuICAgIGFwaVNlcnZpY2UgPSBuZXcgQXBpU2VydmljZShhcGlCYXNlQWRkcmVzcyk7XHJcblxyXG4gICAgLy8gSW5pdGlhbGl6ZSBCb290c3RyYXAgbW9kYWxcclxuICAgIGNvbnN0IG1vZGFsRWxlbWVudCA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKFwiYWRkSW5jcmVtZW50TW9kYWxcIik7XHJcbiAgICBpZiAobW9kYWxFbGVtZW50KSB7XHJcbiAgICAgICAgLy8gQHRzLWlnbm9yZSAtIEJvb3RzdHJhcCBpcyBsb2FkZWQgdmlhIENETlxyXG4gICAgICAgIGFkZEluY3JlbWVudE1vZGFsID0gbmV3IGJvb3RzdHJhcC5Nb2RhbChtb2RhbEVsZW1lbnQpO1xyXG4gICAgfVxyXG5cclxuICAgIC8vIFNldCB1cCBldmVudCBsaXN0ZW5lcnNcclxuICAgIHNldHVwRXZlbnRMaXN0ZW5lcnMoKTtcclxuXHJcbiAgICAvLyBMb2FkIGluaXRpYWwgZGF0YVxyXG4gICAgYXdhaXQgbG9hZEluY3JlbWVudHMoKTtcclxufSk7XHJcblxyXG4vKipcclxuICogU2V0IHVwIGFsbCBldmVudCBsaXN0ZW5lcnNcclxuICovXHJcbmZ1bmN0aW9uIHNldHVwRXZlbnRMaXN0ZW5lcnMoKTogdm9pZCB7XHJcbiAgICAvLyBBZGQgaW5jcmVtZW50IGJ1dHRvblxyXG4gICAgY29uc3QgYWRkQnRuID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoXCJhZGRJbmNyZW1lbnRCdG5cIik7XHJcbiAgICBpZiAoYWRkQnRuKSB7XHJcbiAgICAgICAgYWRkQnRuLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIGNsZWFyRm9ybSgpO1xyXG4gICAgICAgICAgICBhZGRJbmNyZW1lbnRNb2RhbC5zaG93KCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9XHJcblxyXG4gICAgLy8gRm9ybSBzdWJtaXNzaW9uXHJcbiAgICBjb25zdCBmb3JtID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoXCJhZGRJbmNyZW1lbnRGb3JtXCIpIGFzIEhUTUxGb3JtRWxlbWVudDtcclxuICAgIGlmIChmb3JtKSB7XHJcbiAgICAgICAgZm9ybS5hZGRFdmVudExpc3RlbmVyKFwic3VibWl0XCIsIGhhbmRsZUZvcm1TdWJtaXQpO1xyXG4gICAgfVxyXG59XHJcblxyXG4vKipcclxuICogTG9hZCBhbGwgaW5jcmVtZW50cyBmcm9tIHRoZSBBUElcclxuICovXHJcbmFzeW5jIGZ1bmN0aW9uIGxvYWRJbmNyZW1lbnRzKCk6IFByb21pc2U8dm9pZD4ge1xyXG4gICAgdHJ5IHtcclxuICAgICAgICBjb25zdCBpbmNyZW1lbnRzID0gYXdhaXQgYXBpU2VydmljZS5nZXRBbGxJbmNyZW1lbnRzKCk7XHJcbiAgICAgICAgcmVuZGVySW5jcmVtZW50cyhpbmNyZW1lbnRzKTtcclxuICAgIH0gY2F0Y2ggKGVycm9yKSB7XHJcbiAgICAgICAgY29uc29sZS5lcnJvcihcIkZhaWxlZCB0byBsb2FkIGluY3JlbWVudHM6XCIsIGVycm9yKTtcclxuICAgICAgICBzaG93RXJyb3IoXCJGYWlsZWQgdG8gbG9hZCBpbmNyZW1lbnRzLiBQbGVhc2UgcmVmcmVzaCB0aGUgcGFnZS5cIik7XHJcbiAgICB9XHJcbn1cclxuXHJcbi8qKlxyXG4gKiBSZW5kZXIgYWxsIGluY3JlbWVudHMgdG8gdGhlIGRhc2hib2FyZFxyXG4gKi9cclxuZnVuY3Rpb24gcmVuZGVySW5jcmVtZW50cyhpbmNyZW1lbnRzOiBJbmNyZW1lbnRLZXlbXSk6IHZvaWQge1xyXG4gICAgY29uc3QgY29udGFpbmVyID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoXCJpbmNyZW1lbnRzQ29udGFpbmVyXCIpO1xyXG4gICAgaWYgKCFjb250YWluZXIpIHJldHVybjtcclxuXHJcbiAgICAvLyBDbGVhciBleGlzdGluZyBjb250ZW50XHJcbiAgICBjb250YWluZXIuaW5uZXJIVE1MID0gXCJcIjtcclxuXHJcbiAgICAvLyBDaGVjayBpZiB0aGVyZSBhcmUgbm8gaW5jcmVtZW50c1xyXG4gICAgaWYgKGluY3JlbWVudHMubGVuZ3RoID09PSAwKSB7XHJcbiAgICAgICAgY29udGFpbmVyLmlubmVySFRNTCA9ICc8cCBjbGFzcz1cInRleHQtbXV0ZWQgdGV4dC1jZW50ZXIgbXQtNFwiPk5vIGluY3JlbWVudHMgeWV0LiBDbGljayB0aGUgKyBidXR0b24gdG8gYWRkIG9uZSE8L3A+JztcclxuICAgICAgICByZXR1cm47XHJcbiAgICB9XHJcblxyXG4gICAgLy8gQ3JlYXRlIGNhcmRzIGZvciBlYWNoIGluY3JlbWVudFxyXG4gICAgaW5jcmVtZW50cy5mb3JFYWNoKGluY3JlbWVudCA9PiB7XHJcbiAgICAgICAgY29uc3QgY2FyZCA9IGNyZWF0ZUluY3JlbWVudENhcmQoaW5jcmVtZW50KTtcclxuICAgICAgICBjb250YWluZXIuYXBwZW5kQ2hpbGQoY2FyZCk7XHJcbiAgICB9KTtcclxufVxyXG5cclxuLyoqXHJcbiAqIENyZWF0ZSBhbiBpbmNyZW1lbnQgY2FyZCBlbGVtZW50XHJcbiAqL1xyXG5mdW5jdGlvbiBjcmVhdGVJbmNyZW1lbnRDYXJkKGluY3JlbWVudDogSW5jcmVtZW50S2V5KTogSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3QgY2FyZCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJkaXZcIik7XHJcbiAgICBjYXJkLmNsYXNzTmFtZSA9IFwiaW5jcmVtZW50LWNhcmRcIjtcclxuICAgIGNhcmQuZGF0YXNldC5rZXkgPSBpbmNyZW1lbnQuS2V5O1xyXG5cclxuICAgIC8vIEZvcm1hdCB0aGUgdGltZXN0YW1wXHJcbiAgICBjb25zdCBsYXN0VXNlZERhdGUgPSBuZXcgRGF0ZShpbmNyZW1lbnQuTGFzdFVzZWQpO1xyXG4gICAgY29uc3QgZm9ybWF0dGVkRGF0ZSA9IGxhc3RVc2VkRGF0ZS50b1N0cmluZygpO1xyXG5cclxuICAgIGNhcmQuaW5uZXJIVE1MID0gYFxyXG4gICAgICAgIDxkaXYgY2xhc3M9XCJpbmNyZW1lbnQtY2FyZC1oZWFkZXJcIj5cclxuICAgICAgICAgICAgPGgzIGNsYXNzPVwiaW5jcmVtZW50LWNhcmQtdGl0bGVcIj4ke2VzY2FwZUh0bWwoaW5jcmVtZW50LktleSl9PC9oMz5cclxuICAgICAgICAgICAgPGJ1dHRvbiBjbGFzcz1cImluY3JlbWVudC1jYXJkLWRlbGV0ZVwiIHRpdGxlPVwiRGVsZXRlIEluY3JlbWVudFwiIGRhdGEta2V5PVwiJHtlc2NhcGVIdG1sKGluY3JlbWVudC5LZXkpfVwiPlxyXG4gICAgICAgICAgICAgICAgPGkgY2xhc3M9XCJmYS1zb2xpZCBmYS10cmFzaFwiPjwvaT5cclxuICAgICAgICAgICAgPC9idXR0b24+XHJcbiAgICAgICAgPC9kaXY+XHJcbiAgICAgICAgPGRpdiBjbGFzcz1cImluY3JlbWVudC1jYXJkLWJvZHlcIj5cclxuICAgICAgICAgICAgPGRpdiBjbGFzcz1cImluY3JlbWVudC1jYXJkLWZpZWxkXCI+XHJcbiAgICAgICAgICAgICAgICA8cCBjbGFzcz1cImluY3JlbWVudC1jYXJkLWxhYmVsXCI+Q3VycmVudCBWYWx1ZTo8L3A+XHJcbiAgICAgICAgICAgICAgICA8cCBjbGFzcz1cImluY3JlbWVudC1jYXJkLXZhbHVlIGxhcmdlXCI+JHtpbmNyZW1lbnQuUHJldmlvdXNWYWx1ZX08L3A+XHJcbiAgICAgICAgICAgIDwvZGl2PlxyXG4gICAgICAgICAgICA8ZGl2IGNsYXNzPVwiaW5jcmVtZW50LWNhcmQtZmllbGRcIj5cclxuICAgICAgICAgICAgICAgIDxwIGNsYXNzPVwiaW5jcmVtZW50LWNhcmQtdGltZXN0YW1wXCI+TGFzdCBVc2VkIG9uICR7Zm9ybWF0dGVkRGF0ZX08L3A+XHJcbiAgICAgICAgICAgIDwvZGl2PlxyXG4gICAgICAgICAgICA8YnV0dG9uIGNsYXNzPVwiaW5jcmVtZW50LWJ1dHRvblwiIHRpdGxlPVwiSW5jcmVtZW50XCIgZGF0YS1rZXk9XCIke2VzY2FwZUh0bWwoaW5jcmVtZW50LktleSl9XCI+XHJcbiAgICAgICAgICAgICAgICA8aSBjbGFzcz1cImZhLXNvbGlkIGZhLWFycm93LXVwXCI+PC9pPlxyXG4gICAgICAgICAgICA8L2J1dHRvbj5cclxuICAgICAgICA8L2Rpdj5cclxuICAgIGA7XHJcblxyXG4gICAgLy8gQWRkIGRlbGV0ZSBidXR0b24gZXZlbnQgbGlzdGVuZXJcclxuICAgIGNvbnN0IGRlbGV0ZUJ0biA9IGNhcmQucXVlcnlTZWxlY3RvcihcIi5pbmNyZW1lbnQtY2FyZC1kZWxldGVcIikgYXMgSFRNTEJ1dHRvbkVsZW1lbnQ7XHJcbiAgICBpZiAoZGVsZXRlQnRuKSB7XHJcbiAgICAgICAgZGVsZXRlQnRuLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiBoYW5kbGVEZWxldGUoaW5jcmVtZW50LktleSkpO1xyXG4gICAgfVxyXG5cclxuICAgIC8vIEFkZCBpbmNyZW1lbnQgYnV0dG9uIGV2ZW50IGxpc3RlbmVyXHJcbiAgICBjb25zdCBpbmNyZW1lbnRCdG4gPSBjYXJkLnF1ZXJ5U2VsZWN0b3IoXCIuaW5jcmVtZW50LWJ1dHRvblwiKSBhcyBIVE1MQnV0dG9uRWxlbWVudDtcclxuICAgIGlmIChpbmNyZW1lbnRCdG4pIHtcclxuICAgICAgICBpbmNyZW1lbnRCdG4uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IGhhbmRsZUluY3JlbWVudChpbmNyZW1lbnQuS2V5LCBpbmNyZW1lbnRCdG4pKTtcclxuICAgIH1cclxuXHJcbiAgICByZXR1cm4gY2FyZDtcclxufVxyXG5cclxuLyoqXHJcbiAqIEhhbmRsZSBmb3JtIHN1Ym1pc3Npb24gZm9yIGFkZGluZyBhIG5ldyBpbmNyZW1lbnRcclxuICovXHJcbmFzeW5jIGZ1bmN0aW9uIGhhbmRsZUZvcm1TdWJtaXQoZXZlbnQ6IEV2ZW50KTogUHJvbWlzZTx2b2lkPiB7XHJcbiAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cclxuICAgIGNvbnN0IGtleUlucHV0ID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoXCJpbmNyZW1lbnRLZXlcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgIGNvbnN0IHZhbHVlSW5wdXQgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZChcImluY3JlbWVudFByZXZpb3VzVmFsdWVcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuXHJcbiAgICBpZiAoIWtleUlucHV0IHx8ICF2YWx1ZUlucHV0KSByZXR1cm47XHJcblxyXG4gICAgY29uc3QgY29tbWFuZDogSW5jcmVtZW50Q29tbWFuZCA9IHtcclxuICAgICAgICBLZXk6IGtleUlucHV0LnZhbHVlLnRyaW0oKSxcclxuICAgICAgICBQcmV2aW91c1ZhbHVlOiBwYXJzZUludCh2YWx1ZUlucHV0LnZhbHVlLCAxMClcclxuICAgIH07XHJcblxyXG4gICAgLy8gVmFsaWRhdGVcclxuICAgIGlmIChjb21tYW5kLktleS5sZW5ndGggPCAzIHx8IGNvbW1hbmQuS2V5Lmxlbmd0aCA+IDUwKSB7XHJcbiAgICAgICAgc2hvd0Zvcm1FcnJvcihcIktleSBtdXN0IGJlIGJldHdlZW4gMyBhbmQgNTAgY2hhcmFjdGVyc1wiKTtcclxuICAgICAgICByZXR1cm47XHJcbiAgICB9XHJcblxyXG4gICAgdHJ5IHtcclxuICAgICAgICAvLyBDYWxsIEFQSSB0byBjcmVhdGUgaW5jcmVtZW50XHJcbiAgICAgICAgYXdhaXQgYXBpU2VydmljZS51cHNlcnRJbmNyZW1lbnQoY29tbWFuZCk7XHJcblxyXG4gICAgICAgIC8vIENsb3NlIG1vZGFsXHJcbiAgICAgICAgYWRkSW5jcmVtZW50TW9kYWwuaGlkZSgpO1xyXG5cclxuICAgICAgICAvLyBSZWxvYWQgaW5jcmVtZW50c1xyXG4gICAgICAgIGF3YWl0IGxvYWRJbmNyZW1lbnRzKCk7XHJcbiAgICB9IGNhdGNoIChlcnJvcikge1xyXG4gICAgICAgIGNvbnNvbGUuZXJyb3IoXCJGYWlsZWQgdG8gYWRkIGluY3JlbWVudDpcIiwgZXJyb3IpO1xyXG4gICAgICAgIHNob3dGb3JtRXJyb3IoZXJyb3IgaW5zdGFuY2VvZiBFcnJvciA/IGVycm9yLm1lc3NhZ2UgOiBcIkZhaWxlZCB0byBhZGQgaW5jcmVtZW50XCIpO1xyXG4gICAgfVxyXG59XHJcblxyXG4vKipcclxuICogSGFuZGxlIGRlbGV0ZSBidXR0b24gY2xpY2tcclxuICovXHJcbmFzeW5jIGZ1bmN0aW9uIGhhbmRsZURlbGV0ZShrZXk6IHN0cmluZyk6IFByb21pc2U8dm9pZD4ge1xyXG4gICAgaWYgKCFjb25maXJtKGBBcmUgeW91IHN1cmUgeW91IHdhbnQgdG8gZGVsZXRlIHRoZSBpbmNyZW1lbnQgXCIke2tleX1cIj9gKSkge1xyXG4gICAgICAgIHJldHVybjtcclxuICAgIH1cclxuXHJcbiAgICB0cnkge1xyXG4gICAgICAgIGF3YWl0IGFwaVNlcnZpY2UuZGVsZXRlSW5jcmVtZW50KGtleSk7XHJcbiAgICAgICAgYXdhaXQgbG9hZEluY3JlbWVudHMoKTtcclxuICAgIH0gY2F0Y2ggKGVycm9yKSB7XHJcbiAgICAgICAgY29uc29sZS5lcnJvcihcIkZhaWxlZCB0byBkZWxldGUgaW5jcmVtZW50OlwiLCBlcnJvcik7XHJcbiAgICAgICAgc2hvd0Vycm9yKGVycm9yIGluc3RhbmNlb2YgRXJyb3IgPyBlcnJvci5tZXNzYWdlIDogXCJGYWlsZWQgdG8gZGVsZXRlIGluY3JlbWVudFwiKTtcclxuICAgIH1cclxufVxyXG5cclxuLyoqXHJcbiAqIEhhbmRsZSBpbmNyZW1lbnQgYnV0dG9uIGNsaWNrXHJcbiAqL1xyXG5hc3luYyBmdW5jdGlvbiBoYW5kbGVJbmNyZW1lbnQoa2V5OiBzdHJpbmcsIGJ1dHRvbjogSFRNTEJ1dHRvbkVsZW1lbnQpOiBQcm9taXNlPHZvaWQ+IHtcclxuICAgIHRyeSB7XHJcbiAgICAgICAgLy8gRGlzYWJsZSBidXR0b24gYW5kIGtlZXAgZm9jdXMgdG8gc2hvdyBibHVlIG91dGxpbmVcclxuICAgICAgICBidXR0b24uZGlzYWJsZWQgPSB0cnVlO1xyXG4gICAgICAgIGJ1dHRvbi5mb2N1cygpO1xyXG5cclxuICAgICAgICAvLyBDYWxsIEFQSSB0byBpbmNyZW1lbnQgdGhlIHZhbHVlXHJcbiAgICAgICAgYXdhaXQgYXBpU2VydmljZS5pbmNyZW1lbnRWYWx1ZShrZXkpO1xyXG5cclxuICAgICAgICAvLyBSZWxvYWQgaW5jcmVtZW50cyAodGhpcyB3aWxsIHJlLWVuYWJsZSB0aGUgYnV0dG9uIGJ5IHJlLXJlbmRlcmluZylcclxuICAgICAgICBhd2FpdCBsb2FkSW5jcmVtZW50cygpO1xyXG4gICAgfSBjYXRjaCAoZXJyb3IpIHtcclxuICAgICAgICBjb25zb2xlLmVycm9yKFwiRmFpbGVkIHRvIGluY3JlbWVudCB2YWx1ZTpcIiwgZXJyb3IpO1xyXG4gICAgICAgIHNob3dFcnJvcihlcnJvciBpbnN0YW5jZW9mIEVycm9yID8gZXJyb3IubWVzc2FnZSA6IFwiRmFpbGVkIHRvIGluY3JlbWVudCB2YWx1ZVwiKTtcclxuXHJcbiAgICAgICAgLy8gUmUtZW5hYmxlIGJ1dHRvbiBvbiBlcnJvclxyXG4gICAgICAgIGJ1dHRvbi5kaXNhYmxlZCA9IGZhbHNlO1xyXG4gICAgfVxyXG59XHJcblxyXG4vKipcclxuICogQ2xlYXIgdGhlIGZvcm0gYW5kIGhpZGUgZXJyb3IgbWVzc2FnZXNcclxuICovXHJcbmZ1bmN0aW9uIGNsZWFyRm9ybSgpOiB2b2lkIHtcclxuICAgIGNvbnN0IGZvcm0gPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZChcImFkZEluY3JlbWVudEZvcm1cIikgYXMgSFRNTEZvcm1FbGVtZW50O1xyXG4gICAgaWYgKGZvcm0pIHtcclxuICAgICAgICBmb3JtLnJlc2V0KCk7XHJcbiAgICB9XHJcblxyXG4gICAgY29uc3QgZXJyb3JEaXYgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZChcImVycm9yTWVzc2FnZVwiKTtcclxuICAgIGlmIChlcnJvckRpdikge1xyXG4gICAgICAgIGVycm9yRGl2LmNsYXNzTGlzdC5hZGQoXCJkLW5vbmVcIik7XHJcbiAgICAgICAgZXJyb3JEaXYudGV4dENvbnRlbnQgPSBcIlwiO1xyXG4gICAgfVxyXG59XHJcblxyXG4vKipcclxuICogU2hvdyBlcnJvciBtZXNzYWdlIGluIHRoZSBmb3JtXHJcbiAqL1xyXG5mdW5jdGlvbiBzaG93Rm9ybUVycm9yKG1lc3NhZ2U6IHN0cmluZyk6IHZvaWQge1xyXG4gICAgY29uc3QgZXJyb3JEaXYgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZChcImVycm9yTWVzc2FnZVwiKTtcclxuICAgIGlmIChlcnJvckRpdikge1xyXG4gICAgICAgIGVycm9yRGl2LnRleHRDb250ZW50ID0gbWVzc2FnZTtcclxuICAgICAgICBlcnJvckRpdi5jbGFzc0xpc3QucmVtb3ZlKFwiZC1ub25lXCIpO1xyXG4gICAgfVxyXG59XHJcblxyXG4vKipcclxuICogU2hvdyBhIGdlbmVyYWwgZXJyb3IgbWVzc2FnZSAoeW91IGNvdWxkIGVuaGFuY2UgdGhpcyB3aXRoIHRvYXN0IG5vdGlmaWNhdGlvbnMpXHJcbiAqL1xyXG5mdW5jdGlvbiBzaG93RXJyb3IobWVzc2FnZTogc3RyaW5nKTogdm9pZCB7XHJcbiAgICBhbGVydChtZXNzYWdlKTtcclxufVxyXG5cclxuLyoqXHJcbiAqIEVzY2FwZSBIVE1MIHRvIHByZXZlbnQgWFNTXHJcbiAqL1xyXG5mdW5jdGlvbiBlc2NhcGVIdG1sKHRleHQ6IHN0cmluZyk6IHN0cmluZyB7XHJcbiAgICBjb25zdCBkaXYgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwiZGl2XCIpO1xyXG4gICAgZGl2LnRleHRDb250ZW50ID0gdGV4dDtcclxuICAgIHJldHVybiBkaXYuaW5uZXJIVE1MO1xyXG59XHJcbiJdfQ==