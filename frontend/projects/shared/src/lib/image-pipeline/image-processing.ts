/** Larguras geradas para o srcset (ver escopo técnico, seção 4). */
export const IMAGE_WIDTHS = [480, 1200, 2400] as const;

export interface ProcessedVariant {
  width: number;
  height: number;
  blob: Blob;
}

export interface ProcessedImage {
  sha256: string;
  original: { width: number; height: number };
  variants: ProcessedVariant[];
}

/**
 * Redimensiona e converte para WebP. Redesenhar no canvas descarta EXIF/GPS.
 * Pensado para rodar em Web Worker (OffscreenCanvas + createImageBitmap).
 */
export async function processImage(file: Blob, quality = 0.82): Promise<ProcessedImage> {
  const bitmap = await createImageBitmap(file);
  const sha256 = await hashSha256(file);
  const variants: ProcessedVariant[] = [];

  for (const target of IMAGE_WIDTHS) {
    const width = Math.min(target, bitmap.width);
    const height = Math.round((bitmap.height * width) / bitmap.width);
    const canvas = new OffscreenCanvas(width, height);
    canvas.getContext('2d')!.drawImage(bitmap, 0, 0, width, height);
    const blob = await canvas.convertToBlob({ type: 'image/webp', quality });
    variants.push({ width, height, blob });
    if (width === bitmap.width) break; // não amplia além do original
  }

  const original = { width: bitmap.width, height: bitmap.height };
  bitmap.close();
  return { sha256, original, variants };
}

export async function hashSha256(blob: Blob): Promise<string> {
  const digest = await crypto.subtle.digest('SHA-256', await blob.arrayBuffer());
  return Array.from(new Uint8Array(digest))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('');
}
