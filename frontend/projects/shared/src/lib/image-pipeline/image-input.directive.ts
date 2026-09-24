import { Directive, HostListener, input, output } from '@angular/core';

/**
 * Captura imagens por colagem (Ctrl+V), arrastar e soltar ou <input type="file">.
 * Os três caminhos convergem em um único evento `images`.
 *
 * Uso: <div shrImageInput (images)="onImages($event)"></div>
 * Com [listenOnDocument]="true", o Ctrl+V funciona em qualquer lugar da tela.
 */
@Directive({ selector: '[shrImageInput]' })
export class ImageInputDirective {
  readonly listenOnDocument = input(false);
  readonly images = output<File[]>();

  @HostListener('paste', ['$event'])
  onPaste(event: ClipboardEvent): void {
    if (!this.listenOnDocument()) this.handlePaste(event);
  }

  @HostListener('document:paste', ['$event'])
  onDocumentPaste(event: ClipboardEvent): void {
    if (this.listenOnDocument()) this.handlePaste(event);
  }

  @HostListener('dragover', ['$event'])
  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  @HostListener('drop', ['$event'])
  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.emit(Array.from(event.dataTransfer?.files ?? []));
  }

  @HostListener('change', ['$event'])
  onChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target?.type === 'file') {
      this.emit(Array.from(target.files ?? []));
      target.value = '';
    }
  }

  private handlePaste(event: ClipboardEvent): void {
    const files = Array.from(event.clipboardData?.items ?? [])
      .filter((item) => item.kind === 'file' && item.type.startsWith('image/'))
      .map((item) => item.getAsFile())
      .filter((f): f is File => f !== null);
    if (files.length) {
      event.preventDefault();
      this.emit(files);
    }
  }

  private emit(files: File[]): void {
    const images = files.filter((f) => f.type.startsWith('image/'));
    if (images.length) this.images.emit(images);
  }
}
