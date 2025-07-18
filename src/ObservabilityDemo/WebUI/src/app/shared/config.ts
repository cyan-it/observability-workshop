import { Injectable } from '@angular/core';

export interface OtelConfig {
  endpoint: string;
  serviceName: string;
  serviceVersion?: string;
  serviceEnvironment?: string;
  serviceTenant?: string;
}

@Injectable({ providedIn: 'root' })
export class Config {
  apiUrl       = '';
  openTelemetry!: OtelConfig;
}
