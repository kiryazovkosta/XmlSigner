import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Certificate, SignXmlRequest, ProblemDetails } from '../models/certificate.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  getCertificates(): Observable<Certificate[]> {
    return this.http.get<Certificate[]>('/api/certificates');
  }

  signXml(request: SignXmlRequest): Observable<string> {
    return this.http.post('/api/xmlSigner', request, {
      responseType: 'text'
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => this.parseError(error));
      })
    );
  }

  private parseError(error: HttpErrorResponse): string {
    if (error.error) {
      // Try to parse as ProblemDetails
      if (typeof error.error === 'string') {
        try {
          const problemDetails: ProblemDetails = JSON.parse(error.error);
          if (problemDetails.detail) {
            let message = `${problemDetails.title}: ${problemDetails.detail}`;
            if (problemDetails.exceptionMessage) {
              message += ` (${problemDetails.exceptionMessage})`;
            }
            return message;
          }
        } catch {
          // Not JSON, return as-is
          return error.error;
        }
      } else if (error.error.detail) {
        // Already parsed as object
        const pd = error.error as ProblemDetails;
        let message = `${pd.title}: ${pd.detail}`;
        if (pd.exceptionMessage) {
          message += ` (${pd.exceptionMessage})`;
        }
        return message;
      }
    }
    return error.message || 'An unknown error occurred';
  }
}
