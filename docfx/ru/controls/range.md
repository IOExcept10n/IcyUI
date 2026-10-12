---
source-hash: a3c9a45abe99
---

# Элементы диапазона

![Ползунок Volume на 70 и индикатор выполнения Loading на 40 процентах](../../images/controls/range.png)

<xref:Icy.UI.Controls.Slider> позволяет игроку выбрать значение из диапазона, перетаскивая бегунок: громкость,
чувствительность, угол обзора. <xref:Icy.UI.Controls.ProgressBar> показывает значение из диапазона: загрузку, здоровье,
опыт.

## Разметка

[!code-xml[](../../snippets/markup/controls/range.xml)]

Привяжите `Value` к модели представления, чтобы читать или задавать его: `Value="{Binding Volume, Mode=TwoWay}"`.

## Основные свойства

| Свойство | Описание |
|---|---|
| <xref:Icy.UI.Controls.Slider.Minimum> / <xref:Icy.UI.Controls.Slider.Maximum> | Диапазон. |
| <xref:Icy.UI.Controls.Slider.Value> | Текущее значение, всегда внутри диапазона. |
| <xref:Icy.UI.Controls.Slider.ThumbBrush> | Цвет бегунка. |
| <xref:Icy.UI.Controls.ProgressBar.Value> | Прогресс между <xref:Icy.UI.Controls.ProgressBar.Minimum> и <xref:Icy.UI.Controls.ProgressBar.Maximum>. |
| <xref:Icy.UI.Controls.ProgressBar.FillBrush> | Цвет заполненной части. |

## События и команды

<xref:Icy.UI.Controls.Slider.ValueChanged> срабатывает, пока игрок перетаскивает бегунок.

## Стиль

В шаблоне ползунка есть часть `PART_Thumb`, а его визуальные состояния — `Hovered` (более светлый бегунок) и `Focused`.
В шаблоне индикатора выполнения есть часть `PART_Fill`, размер которой следует за значением.

## Клавиатура, геймпад и сенсорный ввод

Ползунок следует за мышью или пальцем при перетаскивании. Он принимает фокус, поэтому до него можно дойти стрелками и
крестовиной геймпада.

> [!NOTE]
> Стрелки и крестовина пока не меняют значение ползунка; они переводят фокус дальше.

## Slider

Значения вне диапазона приводятся к его границам.

## ProgressBar

Только для отображения: не принимает фокус и ввод.

## См. также

- [Списки и выбор](lists.md) — чтобы выбирать из набора значений.
