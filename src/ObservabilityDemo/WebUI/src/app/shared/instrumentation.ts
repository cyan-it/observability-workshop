import { Config, OtelConfig } from './config';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { BatchSpanProcessor } from '@opentelemetry/sdk-trace-base';
import {
  defaultResource,
  resourceFromAttributes
} from '@opentelemetry/resources';
import {
  ATTR_SERVICE_NAME,
  ATTR_SERVICE_VERSION
} from '@opentelemetry/semantic-conventions';
import { getWebAutoInstrumentations } from '@opentelemetry/auto-instrumentations-web';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { ZoneContextManager } from '@opentelemetry/context-zone-peer-dep';
import { W3CTraceContextPropagator } from '@opentelemetry/core';

import {
  LoggerProvider,
  BatchLogRecordProcessor
} from '@opentelemetry/sdk-logs';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-http';

export let tracer:
  | ReturnType<WebTracerProvider['getTracer']>
  | undefined;
export let logger: ReturnType<LoggerProvider['getLogger']>;

export function initInstrumentationFactory(config: Config) {
  return () => {
    const otel: OtelConfig = config.openTelemetry;

    const resource = defaultResource().merge(
      resourceFromAttributes({
        [ATTR_SERVICE_NAME]: otel.serviceName,
        [ATTR_SERVICE_VERSION]: otel.serviceVersion,
        'service.environment': otel.serviceEnvironment,
        'service.tenant': otel.serviceTenant
      })
    );

    const traceExporter = new OTLPTraceExporter({
      url: `${otel.endpoint.replace(/\/+$/, '')}/v1/traces`
    });
    const tracerProvider = new WebTracerProvider({
      resource,
      spanProcessors: [new BatchSpanProcessor(traceExporter)]
    });
    tracerProvider.register({
      contextManager: new ZoneContextManager(),
      propagator: new W3CTraceContextPropagator()
    });
    registerInstrumentations({
      instrumentations: [
        getWebAutoInstrumentations({
          '@opentelemetry/instrumentation-xml-http-request': {
            propagateTraceHeaderCorsUrls: [/.+/g]
          },
          '@opentelemetry/instrumentation-fetch': {
            propagateTraceHeaderCorsUrls: [/.+/g]
          }
        })
      ]
    });
    tracer = tracerProvider.getTracer('http-client');

    const logExporter = new OTLPLogExporter({
      url: `${otel.endpoint.replace(/\/+$/, '')}/v1/logs`
    });
    const loggerProvider = new LoggerProvider({ resource });
    loggerProvider.addLogRecordProcessor(
      new BatchLogRecordProcessor(logExporter)
    );
    logger = loggerProvider.getLogger('http-client-logger');
  };
}
