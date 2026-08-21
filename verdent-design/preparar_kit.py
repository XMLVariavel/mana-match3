"""Recorta o alpha da arte gerada e instala o kit de UI em Assets/Art/UI/BoardV2/Kit."""
import os
from collections import deque

from PIL import Image

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ORIGEM = os.path.join(RAIZ, "verdent-design")
DESTINO = os.path.join(RAIZ, "Assets", "Art", "UI", "BoardV2", "Kit")


def remover_fundo_branco(im, limiar=232):
    """Torna transparente o branco conectado as bordas (nao toca brilhos internos)."""
    largura, altura = im.size
    pixels = im.load()
    visitado = bytearray(largura * altura)
    fila = deque()

    def e_fundo(x, y):
        r, g, b, _ = pixels[x, y]
        return r >= limiar and g >= limiar and b >= limiar

    for x in range(largura):
        for y in (0, altura - 1):
            if not visitado[y * largura + x] and e_fundo(x, y):
                visitado[y * largura + x] = 1
                fila.append((x, y))
    for y in range(altura):
        for x in (0, largura - 1):
            if not visitado[y * largura + x] and e_fundo(x, y):
                visitado[y * largura + x] = 1
                fila.append((x, y))

    while fila:
        x, y = fila.popleft()
        pixels[x, y] = (255, 255, 255, 0)
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if 0 <= nx < largura and 0 <= ny < altura:
                i = ny * largura + nx
                if not visitado[i] and e_fundo(nx, ny):
                    visitado[i] = 1
                    fila.append((nx, ny))
    return im


def cortar(im, corte=8):
    caixa = im.split()[3].point(lambda a: 255 if a > corte else 0).getbbox()
    return im.crop(caixa) if caixa else im


def quadrado(im, margem=0.04):
    lado = int(max(im.size) * (1 + margem * 2))
    tela = Image.new("RGBA", (lado, lado), (0, 0, 0, 0))
    tela.paste(im, ((lado - im.width) // 2, (lado - im.height) // 2), im)
    return tela


def margem_horizontal(im, margem=0.03):
    extra = int(im.height * margem)
    tela = Image.new("RGBA", (im.width, im.height + extra * 2), (0, 0, 0, 0))
    tela.paste(im, (0, extra), im)
    return tela


def salvar(nome, im):
    os.makedirs(DESTINO, exist_ok=True)
    caminho = os.path.join(DESTINO, nome + ".png")
    im.save(caminho)
    print(f"{nome}: {im.size[0]}x{im.size[1]} -> {os.path.relpath(caminho, RAIZ)}")


def abrir(arquivo):
    return Image.open(os.path.join(ORIGEM, arquivo)).convert("RGBA")


def main():
    logo = remover_fundo_branco(abrir("edit_20260821_113024_23689c09.png"))
    salvar("logo_mana", margem_horizontal(cortar(logo)))

    salvar("faixa_pergaminho", cortar(abrir("generate_20260821_112822_ac63a751_1.png")))
    salvar("botao_voltar", quadrado(cortar(abrir("generate_20260821_112721_9cced138_2.png"))))
    salvar("botao_circular", quadrado(cortar(abrir("generate_20260821_112721_5b2d36a3_3.png"))))
    salvar("badge_livro", cortar(abrir("generate_20260821_113059_783a7312.png")))
    salvar("moeda", quadrado(cortar(abrir("generate_20260821_113059_047fa7e5_1.png"))))


if __name__ == "__main__":
    main()
