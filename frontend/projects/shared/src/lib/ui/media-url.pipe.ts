import { Pipe, PipeTransform } from '@angular/core';

/** Monta a URL pública de uma variante (S3 + CloudFront): {base}/{key}-{width}.webp */
@Pipe({ name: 'mediaUrl' })
export class MediaUrlPipe implements PipeTransform {
  transform(key: string | null | undefined, baseUrl: string, width = 1200): string {
    return key ? `${baseUrl}/${key}-${width}.webp` : '';
  }
}
