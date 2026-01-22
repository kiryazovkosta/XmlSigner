import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from './services/api.service';
import { Certificate } from './models/certificate.model';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private apiService = inject(ApiService);

  certificates = signal<Certificate[]>([]);
  selectedCertificate = signal<Certificate | null>(null);
  xmlMessage = signal('');
  signedXml = signal('');
  loading = signal(false);
  error = signal('');

  ngOnInit() {
    this.loadCertificates();
  }

  loadCertificates() {
    this.loading.set(true);
    this.error.set('');

    this.apiService.getCertificates().subscribe({
      next: (certs) => {
        this.certificates.set(certs);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load certificates: ' + err.message);
        this.loading.set(false);
      }
    });
  }

  selectCertificate(cert: Certificate) {
    this.selectedCertificate.set(cert);
  }

  signXml() {
    const cert = this.selectedCertificate();
    const message = this.xmlMessage();

    if (!cert || !message) {
      this.error.set('Please select a certificate and enter XML message');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.signedXml.set('');

    this.apiService.signXml({ certificate: cert, message }).subscribe({
      next: (result) => {
        this.signedXml.set(result);
        this.loading.set(false);
      },
      error: (err: string) => {
        this.error.set(err);
        this.loading.set(false);
      }
    });
  }
}
