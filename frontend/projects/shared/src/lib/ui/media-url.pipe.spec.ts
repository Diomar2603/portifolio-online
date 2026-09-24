import { MediaUrlPipe } from './media-url.pipe';

describe('MediaUrlPipe', () => {
  const pipe = new MediaUrlPipe();

  it('monta a URL da variante', () => {
    expect(pipe.transform('abc', 'https://media.x.dev', 480)).toBe('https://media.x.dev/abc-480.webp');
  });

  it('retorna vazio sem chave', () => {
    expect(pipe.transform(null, 'https://media.x.dev')).toBe('');
  });
});
