import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Certificate, SignXmlRequest } from '../models/certificate.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  getCertificates(): Observable<Certificate[]> {
    return this.http.get<Certificate[]>('/api/certificates');
  }

  signXml(request: SignXmlRequest): Observable<string> {
    return this.http.post('/api/xmlSigner', request, {
      responseType: 'text'
    });
  }
}
