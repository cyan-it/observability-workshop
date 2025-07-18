import {
  HttpEvent,
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpResponse,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { tracer, logger } from './shared/instrumentation';

export const HttpRequestTracingInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  return new Observable(observer => {
    tracer!.startActiveSpan('http-request', span => {
      span.setAttribute('url', req.url);

      logger.emit({
        body: `Calling ${req.method} ${req.url}`,
        severityNumber: 9,
        attributes: { url: req.url, method: req.method }
      });

      next(req)
        .pipe(
          tap({
            next: (event) => {
              if (event instanceof HttpResponse) {
                span.setAttribute('http.status_code', event.status);

                logger.emit({
                  body: `Response ${event.status} for ${req.url}`,
                  severityNumber: 9,
                  attributes: { status: event.status }
                });

                span.end();
              }
            },
            error: (error: HttpErrorResponse) => {
              span.recordException(error);

              logger.emit({
                body: `Error ${error.status} on ${req.url}: ${error.message}`,
                severityNumber: 17,
                attributes: { status: error.status }
              });

              span.end();
              observer.error(error);
            }
          })
        )
        .subscribe({
          next: evt => observer.next(evt),
          error: err => observer.error(err),
          complete: () => observer.complete()
        });
    });
  });
};
