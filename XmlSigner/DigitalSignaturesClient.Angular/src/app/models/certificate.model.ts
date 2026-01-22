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

export interface ProblemDetails {
  status: number;
  type: string;
  title: string;
  detail: string;
  instance: string;
  exceptionMessage?: string;
  exceptionType?: string;
}
