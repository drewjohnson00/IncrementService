/**
 * API Service for communicating with the IncrementService backend
 */

import { IncrementKey, IncrementCommand, ErrorResponse } from './models.js';

export class ApiService {
    private baseUrl: string;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    /**
     * Get all increments
     */
    async getAllIncrements(): Promise<IncrementKey[]> {
        const response = await fetch(`${this.baseUrl}/Increments`, {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!response.ok) {
            throw await this.handleError(response);
        }

        return await response.json();
    }

    /**
     * Get a specific increment by key
     */
    async getIncrement(key: string): Promise<IncrementKey> {
        const response = await fetch(`${this.baseUrl}/Increments/${encodeURIComponent(key)}`, {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!response.ok) {
            throw await this.handleError(response);
        }

        return await response.json();
    }

    /**
     * Create or update an increment
     */
    async upsertIncrement(command: IncrementCommand): Promise<IncrementKey> {
        const response = await fetch(`${this.baseUrl}/Increments`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(command)
        });

        if (!response.ok) {
            throw await this.handleError(response);
        }

        return await response.json();
    }

    /**
     * Increment the value for a specific key
     */
    async incrementValue(key: string): Promise<IncrementKey> {
        const response = await fetch(`${this.baseUrl}/Increments/${encodeURIComponent(key)}`, {
            method: 'POST',
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!response.ok) {
            throw await this.handleError(response);
        }

        return await response.json();
    }

    /**
     * Delete an increment
     */
    async deleteIncrement(key: string): Promise<void> {
        const response = await fetch(`${this.baseUrl}/Increments/${encodeURIComponent(key)}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw await this.handleError(response);
        }
    }

    /**
     * Handle API errors
     */
    private async handleError(response: Response): Promise<Error> {
        try {
            const errorResponse: ErrorResponse = await response.json();
            const message = errorResponse.ValidationErrors && errorResponse.ValidationErrors.length > 0
                ? `${errorResponse.Message}\n- ${errorResponse.ValidationErrors.join('\n- ')}`
                : errorResponse.Message;
            return new Error(message);
        } catch {
            return new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
    }
}
