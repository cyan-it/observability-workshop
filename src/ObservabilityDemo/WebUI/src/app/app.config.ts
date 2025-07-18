import { APP_INITIALIZER, ApplicationConfig, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { HttpRequestTracingInterceptor } from './http-request-tracing.interceptor';
import { Config, OtelConfig } from './shared/config';
import { provideAnimations } from '@angular/platform-browser/animations';
import { initInstrumentationFactory } from './shared/instrumentation';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(
      withFetch(),
      withInterceptors([HttpRequestTracingInterceptor])
    ),

    {
      provide: APP_INITIALIZER,
      useFactory: () => {
        const cfg = inject(Config);
        const url = `${window.location.origin}/configuration/appsettings.json`;
        return () =>
          fetch(url)
            .then(r => r.json())
            .then(json => {
              cfg.apiUrl = json.apiUrl;
              const o = json.OpenTelemetry;
              cfg.openTelemetry = {
                endpoint:          o.Endpoint,
                serviceName:       o.ServiceName,
                serviceVersion:    o.ServiceVersion,
                resourceAttributes: o.ResourceAttributes,
              } as OtelConfig;
              initInstrumentationFactory(cfg)();
            });
      },
      multi: true,
    },
  ],
};
