import { Component } from '@angular/core';

@Component({
  selector: 'site-footer',
  template: `
    <footer class="mx-auto max-w-6xl px-6 py-10 text-sm text-moss/70">
      © {{ year }} · Feito com Angular e .NET
    </footer>
  `,
})
export class Footer {
  protected readonly year = new Date().getFullYear();
}
