# Landing page VENDEX

Página de vendas estática (HTML/CSS/JS puro, sem build), pronta para publicar gratuitamente no Cloudflare Pages.

## Antes de publicar

1. Troque os prints de tela: os arquivos em `assets/screenshot-*.svg` são placeholders (mockups desenhados, não são a tela real). Tire capturas de tela reais do Vendex, salve como `.png` ou `.jpg` na pasta `assets/` e ajuste os `src` no `index.html` (seção `.carousel-track`).
2. Confira o link de checkout: os botões usam `https://pay.hotmart.com/F106817852V?checkoutMode=2`. Se o código do produto/oferta mudar na Hotmart, atualize o `href` nos três botões (`class="hotmart-fb hotmart__button-checkout"`).

## Deploy no Cloudflare Pages (grátis)

**Opção 1 — arrastar e soltar (mais simples, sem Git):**

1. Acesse https://dash.cloudflare.com → **Workers & Pages** → **Create** → aba **Pages** → **Upload assets**.
2. Dê um nome ao projeto (ex.: `vendex`) e arraste a pasta `landing-page` (o conteúdo dela, não a pasta em si) para a área de upload.
3. Clique em **Deploy**. Em segundos você recebe um link do tipo:
   ```
   https://vendex.pages.dev
   ```
4. Para atualizar depois, volte na mesma tela do projeto e faça upload novamente (ou use a Opção 2 com Git para atualizar automaticamente a cada commit).

**Opção 2 — conectar ao GitHub (deploy automático a cada push):**

1. Suba a pasta `landing-page` para um repositório no GitHub (pode ser separado deste repo do sistema, ou um repo próprio só para a página).
2. No dashboard Cloudflare: **Workers & Pages** → **Create** → **Pages** → **Connect to Git**.
3. Selecione o repositório. Em *Build settings*, deixe **Framework preset: None** e **Build output directory: /** (é site estático, sem build).
4. Deploy. Toda vez que você der push, o Cloudflare publica a nova versão automaticamente.

## Links curtos de download (`_redirects`)

O arquivo `_redirects` faz o Cloudflare Pages redirecionar `/download` e `/manual` para os
assets publicados em `vendex-releases` no GitHub — assim o link fica com a cara do seu
domínio (`https://SEU-PROJETO.pages.dev/download`) em vez de um link cru do GitHub.
**Toda vez que publicar uma versão nova**, atualize as duas URLs de destino nesse arquivo
pra apontar pra tag nova, e faça o redeploy — senão o link curto continua entregando a
versão antiga.

## Domínio próprio (opcional)

O link `https://<projeto>.pages.dev` já é gratuito e definitivo. Se quiser um domínio próprio (ex. `vendexpdv.com.br`):

1. Registre o domínio em um registrador (Registro.br, etc. — isso tem custo próprio, fora do Cloudflare).
2. No projeto Pages → **Custom domains** → adicione o domínio.
3. Aponte os nameservers do domínio para a Cloudflare (o próprio painel mostra o passo a passo). A conexão do domínio ao Pages em si é gratuita.
