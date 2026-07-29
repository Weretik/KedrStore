```typescript
import { CATALOG_CATEGORY_IMAGES } from './catalog-images.config';

type CatalogMenuLink = {
    label: string;
    slug: string;
    image?: string;
};

type CatalogMenuSection = {
    label: string;
    slug: string;
    icon: string;
    image?: string;
    items: readonly CatalogMenuLink[];
};

export const CATALOG_ROOT_CATEGORIES = {
    hardware: {
        label: 'Фурнітура',
        slug: 'furnitra-5513',
        icon: 'ri-key-2-line',
    },
    doors: {
        label: 'Двері',
        slug: 'dveri-7226',
        icon: 'ri-door-open-line',
    },
} as const;

export const CATALOG_HARDWARE_SECTIONS = {
    hinges: {
        label: 'Завіси',
        slug: 'zavisi-900001',
        icon: 'ri-arrow-left-right-line',
        image: CATALOG_CATEGORY_IMAGES.hinges,
        items: [
            {
                label: 'Накладні(метелик)',
                slug: 'kedr-zavisi-nakladni-meteliki-dlia-mizhkimnatnikh-dverei-1707',
            },
            {
                label: 'Ввертні та приварні',
                slug: 'kedr-zavisi-vvertni-ta-kovpachki-do-nikh-privarni-zavisi-4457',
            },
            {
                label: 'Врізні',
                slug: 'kedr-zavisi-vrizni-dlia-mizhkimnatnikh-dverei-6139',
            },
        ],
    },
    locks: {
        label: 'Замки',
        slug: 'zamki-900002',
        icon: 'ri-lock-2-line',
        image: CATALOG_CATEGORY_IMAGES.locks,
        items: [
            {
                label: 'Навісні та велозамки',
                slug: 'kedr-zamki-navisni-ta-velozamki-1304',
            },
            {
                label: 'Комплекти з ручками',
                slug: 'kedr-komplekti-zamki-z-ruchkami-2716',
            },
            {
                label: 'Накладні',
                slug: 'kedr-zamki-nakladni-2722',
            },
            {
                label: 'Сувальдні та з хрестообр. ключем',
                slug: 'kedr-zamki-suvaldni-ta-z-khrestoobr.-kliuchem-2775',
            },
            {
                label: 'Врiзні під циліндр',
                slug: 'kedr-zamki-vrizni-pid-tsilindr-5851',
            },
        ],
    },
    handles: {
        label: 'Ручки',
        slug: 'ruchki-900003',
        icon: 'ri-bar-chart-horizontal-line',
        image: CATALOG_CATEGORY_IMAGES.handles,
        items: [
            {
                label: 'На розетцi (Kevlar)',
                slug: 'kedr-ruchki-na-rozettsi-seriia-kevlar-26949',
            },
            {
                label: 'На планці',
                slug: 'kedr-ruchki-na-plantsi-5853',
            },
            {
                label: 'На розетцi (R-08/R-10)',
                slug: 'kedr-ruchki-na-rozettsi-seriia-standart-r-08-r-10-5915',
            },
            {
                label: 'На розетцi (HRoz-06/HRoz-07)',
                slug: 'kedr-ruchki-na-rozettsi-seriia-ekonom-hroz-06-hroz-07-5854',
            },
            {
                label: 'На розетцi преміум (Genrich)',
                slug: 'kedr-ruchki-na-rozettsi-seriia-premium-genrich-5904',
            },
            {
                label: 'На розетцi (Ultra)',
                slug: 'kedr-ruchki-na-rozettsi-seriia-ultra-6982',
            },
            {
                label: 'З нержавiйки',
                slug: 'kedr-ruchki-z-nerzhaviiki-5999',
            },
            {
                label: 'Ручки-кноби',
                slug: 'kedr-ruchki-knobi-6488',
            },
        ],
    },
    cylinders: {
        label: 'Циліндри',
        slug: 'tsilindri-900004',
        icon: 'ri-key-2-line',
        image: CATALOG_CATEGORY_IMAGES.cylinders,
        items: [
            {
                label: 'серії BRASS KEY Латунь',
                slug: 'kedr-tsilindri-seriyi-brass-key-latun-2680',
            },
            {
                label: 'серії SMART',
                slug: 'kedr-tsilindri-seriyi-smart-26929',
            },
            {
                label: 'серії GWK',
                slug: 'kedr-tsilindri-seriyi-gwk-26930',
            },
            {
                label: 'серії ZINK під шток',
                slug: 'kedr-tsilindri-seriyi-zink-pid-shtok-27124',
            },
            {
                label: 'серії ZINK',
                slug: 'kedr-tsilindri-seriyi-zink-5852',
            },
            {
                label: 'серії ZINK PLK',
                slug: 'kedr-tsilindri-seriyi-zink-plk-4555',
            },
            {
                label: 'серії ALU',
                slug: 'kedr-tsilindri-seriyi-alu-6560',
            },
        ],
    },
    interiorMechanisms: {
        label: 'Міжкімнатні механізми',
        slug: 'mizhkimnatni-mekhanizmi-900005',
        icon: 'pi pi-cog',
        image: CATALOG_CATEGORY_IMAGES.interiorMechanisms,
        items: [
            {
                label: 'з магнітною защіпкою',
                slug: 'kedr-mizhkimnatni-mekhanizmi-z-magnitnoiu-zashchipkoiu-2197',
            },
            {
                label: 'заскочки / засувки',
                slug: 'kedr-mizhkimnatni-zaskochki-zasuvki-2321',
            },
            {
                label: 'з металевою защіпкою',
                slug: 'kedr-mizhkimnatni-mekhanizmi-z-metalevoiu-zashchipkoiu-5273',
            },
            {
                label: 'з кевларовою защіпкою',
                slug: 'kedr-mizhkimnatni-mekhanizmi-z-kevlarovoiu-zashchipkoiu-6108',
            },
        ],
    },
    other: {
        label: 'Інше',
        slug: 'inshe-900006',
        icon: 'pi pi-ellipsis-h',
        image: CATALOG_CATEGORY_IMAGES.other,
        items: [
            {
                label: 'Броненакладки на циліндр',
                slug: 'kedr-bronenakladki-na-tsilindr-ta-nakladki-na-suvaldni-zamki-1230',
            },
            {
                label: 'Ущільнювач',
                slug: 'kedr-ushchilniuvach-1440',
            },
            {
                label: 'Відбійники',
                slug: 'kedr-vidbiiniki-3783',
            },
            {
                label: 'Комплектуючі',
                slug: 'kedr-komplektuiuchi-5625',
            },
            {
                label: 'Засувки і шпінгалети',
                slug: 'kedr-zasuvki-i-shpingaleti-dlia-vkhidnikh-ta-mizhkimnatnikh-dverei-5912',
            },
            {
                label: 'Розсувнi системи',
                slug: 'kedr-rozsuvni-sistemi-6295',
            },
            {
                label: 'Дотягувачі',
                slug: 'kedr-dotiaguvachi-5962',
            },
            {
                label: 'Вiчка двернi',
                slug: 'kedr-vichka-dverni-5957',
            },
        ],
    },
} as const satisfies Record<string, CatalogMenuSection>;

export const CATALOG_DOOR_SECTIONS = {
    entranceDoors: {
        label: 'Вхідні двері',
        slug: 'vkhidni-dveri-910001',
        icon: 'ri-shield-keyhole-line',
        items: [
            { label: 'Steelguard', slug: 'dveri-steelguard-7258' },
            { label: 'Lacosta', slug: 'dveri-lacosta-7259' },
            { label: 'Sova', slug: 'dveri-sova-19425' },
            { label: 'MSM', slug: 'dveri-msm-27083' },
            { label: 'Maximum', slug: 'dveri-tsdkh-8209' },
        ],
    },
    interiorDoors: {
        label: 'Міжкімнатні двері',
        slug: 'mizhkimnatni-dveri-910002',
        icon: 'ri-door-open-line',
        items: [
            { label: 'Korfad', slug: 'm_korfad-8349' },
            { label: 'Leador', slug: 'm_leador-8344' },
            { label: 'Darumi', slug: 'm_darumi-19424' },
            { label: 'Syndicate', slug: 'dveri-sindikat-26971' },
        ],
    },
} as const;

```
