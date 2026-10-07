# Workflows do GitHub Actions (v3 · AWS)

Por segurança, o Claude não consegue gravar dentro de `.github/workflows/`.
Mova estes três arquivos para lá e apague esta pasta:

```powershell
Move-Item infra\github-workflows\*.yml .github\workflows\
Remove-Item -Recurse infra\github-workflows
```
