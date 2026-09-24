import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Media, processImage } from '@portfolio/shared';
import { environment } from '../../../environments/environment';

interface UploadTicket {
  id: string;
  uploadUrls: { width: number; url: string }[];
}

/**
 * Fluxo: processa no navegador → pede URLs pré-assinadas → PUT direto no R2 → confirma.
 * TODO: mover processImage para um Web Worker.
 */
@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/admin/media`;

  async upload(file: File, alt: string): Promise<Media> {
    const processed = await processImage(file);
    const ticket = await firstValueFrom(
      this.http.post<UploadTicket>(this.base, {
        sha256: processed.sha256,
        width: processed.original.width,
        height: processed.original.height,
        alt,
        variants: processed.variants.map((v) => ({ width: v.width, bytes: v.blob.size })),
      }),
    );

    await Promise.all(
      ticket.uploadUrls.map(({ width, url }) => {
        const variant = processed.variants.find((v) => v.width === width)!;
        return fetch(url, { method: 'PUT', body: variant.blob, headers: { 'Content-Type': 'image/webp' } });
      }),
    );

    return firstValueFrom(this.http.post<Media>(`${this.base}/${ticket.id}/confirm`, {}));
  }
}
