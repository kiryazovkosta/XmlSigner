export interface Certificate {
  subject: string;
  issuer: string;
  notAfter: string;
  serialNumber: string;
}

export interface SignXmlRequest {
  certificate: Certificate;
  message: string;
}
