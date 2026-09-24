import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from './layout/footer';
import { Header } from './layout/header';

@Component({
  selector: 'site-root',
  imports: [RouterOutlet, Header, Footer],
  templateUrl: './app.html',
})
export class App {}
