/**
 * TypeScript interfaces matching the backend DTOs
 */

export interface IncrementKey {
    Key: string;
    PreviousValue: number;
    LastUsed: string; // ISO 8601 date string
}

export interface IncrementCommand {
    Key: string;
    PreviousValue?: number;
}

export interface ErrorResponse {
    Message: string;
    ValidationErrors?: string[];
}
