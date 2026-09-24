import { Component, inject, signal } from '@angular/core';
import { ImageInputDirective } from '@portfolio/shared';
import { MediaService } from '../../core/api/media.service';

@Component({
  selector: 'adm-projects-page',
  imports: [ImageInputDirective],
  template: `
    <h1 class="text-2xl font-semibold">Projetos</h1>

    <label
      shrImageInput
      [listenOnDocument]="true"
      (images)="onImages($event)"
      class="mt-6 flex cursor-pointer flex-col items-center justify-center rounded-2xl border-2 border-dashed border-slate-300 p-10 text-slate-500 hover:border-emerald-600"
    >
      <span>Arraste imagens aqui, clique para escolher ou cole com Ctrl+V</span>
      <input type="file" accept="image/*" multiple class="hidden" />
    </label>

    <ul class="mt-4 space-y-1 text-sm">
      @for (item of queue(); track item.name) {
        <li>{{ item.name }} — {{ item.status }}</li>
      }
    </ul>
  `,
})
export class ProjectsPage {
  private readonly media = inject(MediaService);
  protected readonly queue = signal<{ name: string; status: string }[]>([]);

  protected async onImages(files: File[]): Promise<void> {
    for (const file of files) {
      const name = file.name || `colada-${Date.now()}.png`;
      this.setStatus(name, 'enviando…');
      try {
        // TODO: pedir texto alternativo (obrigatório) antes do envio
        await this.media.upload(file, name);
        this.setStatus(name, 'pronto');
      } catch {
        this.setStatus(name, 'erro');
      }
    }
  }

  private setStatus(name: string, status: string): void {
    this.queue.update((q) => [...q.filter((i) => i.name !== name), { name, status }]);
  }
}
