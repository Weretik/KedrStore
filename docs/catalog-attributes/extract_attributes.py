"""Extract catalog attributes using the approved dictionaries and regular expressions.

Python 3.10+; standard library only. No database or network calls.
python extract_attributes.py --products Products.json --categories ProductCategories.json
Use --dictionaries dictionaries.json and --outdir PATH when needed.
CSV null values are written as the literal null. original_name is never modified.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import html
import json
import re
from collections import Counter, defaultdict
from decimal import Decimal
from pathlib import Path

VERSION = "2.0"
SUPPORTED = "approved"
UNRESOLVED = "unresolved"
LETTERS = "A-Za-zА-Яа-яІіЇїЄєҐґ"
NUMBER = r"\d+(?:[.,]\d+)?"
DIMENSION = re.compile(rf"{NUMBER}\s*[*xх×]\s*{NUMBER}", re.I)
EVENTS = []
COLOR_RULES = []
OTHER_RULES = []
PRODUCT_TYPE_RULES = []
ENTRY = set()
INTERIOR = set()
CYLINDER = set()
SMART = 26929
SMART_CATEGORIES = {SMART}
COLUMNS = ["product_id","category_id","category_path","original_name","base_name","size",
    "size_length","size_width","size_thickness","size_diameter","color_raw","color_code","color_name",
    "color_secondary_code","color_secondary_name","side",
    "extra_attributes","needs_review","review_reason","notes"]


def is_smart_product(p):
    return p.get("CategoryId") in SMART_CATEGORIES or bool(re.search(r"(?:циліндр|цилиндр)\s+(?:електрон{1,2}ий|электрон{1,2}ый)\s+L4-",p.get("Name") or "",re.I))


def rx(pattern):
    return re.compile(pattern, re.I)

def bounded(pattern):
    return rf"(?<![{LETTERS}0-9])(?:{pattern})(?![{LETTERS}0-9])"

def phrase_pattern(text):
    # Recognition only. Raw substrings retain every original code point.
    pattern = re.escape(text).replace(r"\ ", r"\s+")
    pattern = pattern.replace("і", "[іi]").replace("І", "[ІI]")
    return pattern

def code_pattern(text):
    chars = {"A":"[AА]", "B":"[BВ]", "C":"[CС]", "E":"[EЕ]", "H":"[HН]", "K":"[KК]",
             "M":"[MМ]", "O":"[OО]", "P":"[PР]", "T":"[TТ]", "X":"[XХ]"}
    return "".join(chars.get(ch, re.escape(ch)) for ch in text)

def folded(text):
    text = text.casefold().replace("i", "і")
    return re.sub(r"\s+", " ", text).strip()

def event(p, attribute, match, code=None, name=None, *, status=SUPPORTED, scope="general",
          note="", sources=("export",), candidate=None, components=None):
    start, end = match if isinstance(match, tuple) else match.span()
    raw = p["Name"][start:end]
    assert raw and 0 <= start < end <= len(p["Name"])
    EVENTS.append(dict(product_id=p["Id"], attribute=attribute, scope=scope, raw_value=raw,
        start=start, end=end, normalized_code=code, normalized_value_uk=name, status=status,
        note=note, source_refs=list(sources), candidate=candidate, components=components))
    return EVENTS[-1]

def overlap(start, end, spans):
    return any(start < b and a < end for a,b in spans)

def colour_scope(p):
    if p["CategoryId"] in ENTRY:
        return "entrance_door_finish"
    if p["CategoryId"] in INTERIOR:
        return "interior_door_decor"
    return "hardware_finish"

def colour_regions(p):
    name, cid = p["Name"], p["CategoryId"]
    if is_smart_product(p):
        return []
    if cid in ENTRY:
        dimension = DIMENSION.search(name)
        if not dimension:
            return []
        tail = dimension.end()
        side = re.match(r"\s*[LR](?:\s+|$)", name[tail:])
        if side:
            tail += side.end()
        ending = re.search(r"\s*\+\s*2-й\s+замок", name[tail:])
        return [(tail, tail + ending.start() if ending else len(name))]
    if cid in INTERIOR:
        # The model is before a size/first comma. Syndicate places its decor before size.
        if re.search(r"^(?:Робота|Ремонт|Доставка)", name, re.I):
            return []
    return [(0, len(name))]

def supported_event(p, d, match, *, scope="general", attribute=None):
    return event(p, attribute or d["attribute"], match, d["normalized_code"], d["normalized_value_uk"],
                 scope=scope, status=d["status"], note=d["note"], sources=d["source_refs"], candidate=d["candidate"])

def extract(p):
    name, cid = p["Name"], p["CategoryId"]
    own_start = len(EVENTS)
    blocked_colour = []
    # Quoted model names and explicit component colours are not primary colours.
    for match in rx(r'"Бронза"|«Бронза»|білий\s+цинк').finditer(name):
        blocked_colour.append(match.span())
    if re.search(r"дерев['’]яна",name,re.I):
        for match in rx(bounded(r"Дуб|Вишня|Горіх")).finditer(name):
            event(p,"wood_species_or_decor",match,status=UNRESOLVED,candidate=match.group(),
                  note="У деревянной ручки порода и название отделки не различены. Не назначать автоматически цвет.")
            blocked_colour.append(match.span())
    for match in rx(r"біла\s+резинка").finditer(name):
        event(p, "component_color", match, "WH", "Білий", scope="rubber_part",
              note="Цвет резинки, не всего упора.")
        blocked_colour.append(match.span())
    infill_patterns = [
        ("GLASS_BLACK", "Чорне скло", r"(?:скло|cкло)\s+чорне|Чорне\s+скло"),
        ("GLASS_WHITE", "Біле скло", r"скло\s+біле"),
        ("INFILL_SATIN_WHITE", "Сатин білий", r"Сатин\s+б[іi]лий"),
        ("INFILL_SATIN_BRONZE", "Сатин бронза", r"Сатин\s+бронза"),
        ("GLASS_BRONZE", "Бронзове скло", r"скло\s+бронза"),
        ("MIRROR_BRONZE_TRIPLEX", "Двостороннє бронзове триплекс-дзеркало", r"Двостороннє\s+бронзове\s+триплекс\s+дзеркало"),
        ("MIRROR_GRAPHITE_TRIPLEX", "Двостороннє графітове триплекс-дзеркало", r"Двостороннє\s+граф[іi]т\s+триплекс\s+дзеркало"),
        ("MIRROR_GRAPHITE", "Двостороннє графітове дзеркало", r"Двостороннє\s+граф[іi]т\s+дзеркало"),
        ("LACOBEL_BLACK", "Lacobel чорний лак", r"Lacobel\s+чорний\s+лак"),
        ("SOLID", "Глухе полотно", r"глухе|глуха"),
        ("GLASS_SATIN", "Скло сатин", r"скло\s+сатин"),
    ]
    if cid in INTERIOR:
        for code, label, pattern in infill_patterns:
            for match in rx(bounded(pattern)).finditer(name):
                if not overlap(*match.span(), blocked_colour):
                    event(p, "door_infill", match, code, label, scope="door_infill")
                    blocked_colour.append(match.span())
        for match in rx(bounded(r"[CС]ірий\s+графіт|Сатин")).finditer(name):
            if not overlap(*match.span(), blocked_colour):
                event(p, "unassigned_finish", match, status=UNRESOLVED, scope="interior_door_second_finish",
                      candidate="Сірий графіт" if "графіт" in match.group().lower() else "Сатин",
                      note="Указано после модели/декора, но нет явного слова скло или названия компонента.")
                blocked_colour.append(match.span())
        for match in rx(bounded("BLK|М1")).finditer(name):
            event(p, "unresolved_marker", match, status=UNRESOLVED, scope="door_model_or_infill",
                  note="Код после модели/декора: не назначать по нему чёрный цвет полотну.")
            blocked_colour.append(match.span())
    # Preserve ambiguous finish strings as a whole, rather than resolving their pieces.
    for match in rx(bounded(code_pattern("S") + r"\s*/\s*" + code_pattern("CR"))).finditer(name):
        event(p, "color", match, status=UNRESOLVED, scope=colour_scope(p),
              note="S/CR в каталоге подписан как хром, но составной код нельзя уверенно приравнять к SN/CP.",
              sources=("catalog_2020",), candidate="Хром або сатин / хром")
        blocked_colour.append(match.span())

    candidates=[]
    for d in COLOR_RULES:
        if d.get("eligible_name_pattern") and not re.search(d["eligible_name_pattern"],name,re.I):
            d=dict(d,status=UNRESOLVED,normalized_code=None,normalized_value_uk=None,
                scope_override="unverified_brand_or_series_finish",
                note="Расшифровка подобрана ассистентом для других проверенных серий; в этой серии значение кода не подтверждено.")
        for match in d["compiled"].finditer(name):
            start,end = match.span()
            if not any(start >= a and end <= b for a,b in colour_regions(p)):
                continue
            if overlap(start,end,blocked_colour):
                continue
            raw_key = folded(match.group())
            if d["kind"] == "colour_code":
                if cid in INTERIOR and re.search(r"Дверне\s+полотно|Каркасно-щитові\s+двері",name,re.I):
                    dimension=DIMENSION.search(name)
                    if dimension and start<dimension.end():
                        continue  # A coded model prefix before the leaf size is not its finish.
                if re.match(r"-\d", name[end:]):
                    continue  # NP-02, SC-04 and other model prefixes are not colour codes.
                if raw_key == "s" and (re.search(r"(?:\d+[BC]|PE\d+)-$", name[:start], re.I) or re.search(r"Полицетримач\s+Пелікан\s+$", name[:start], re.I)):
                    continue  # 410B-S / 410C-S: model suffix.
                if raw_key == "g" and re.search(r"^\s+(?:410[BC]-S|100\s+заскочка)", name[end:], re.I):
                    continue  # series marker before model.
                if raw_key in ("sc",) and re.match(r"-\d", name[end:]):
                    continue  # SCALEA SC-04 is a model, not a colour.
                if raw_key == "pb" and is_smart_product(p):
                    continue
            if d["kind"] == "unresolved_colour" and folded(match.group()) in ("sb", "ss", "gr", "ww", "ac", "orb"):
                if re.match(r"-\d", name[end:]):
                    continue
            candidates.append((start,end,d,match))
    # Leftmost, longest match: no matching of white inside a named white decor.
    selected=[]
    for start,end,d,match in sorted(candidates, key=lambda item:(item[0], -(item[1]-item[0]), item[2]["status"] != UNRESOLVED)):
        if not overlap(start,end,[item[0:2] for item in selected]):
            selected.append((start,end,d,match))
    colour_events=[]
    for start,end,d,match in selected:
        ce = supported_event(p,d,match,scope=d.get("scope_override") or colour_scope(p))
        if re.fullmatch(code_pattern("CP"),match.group(),re.I) and "9011" in name:
            ce.update(normalized_code=None, normalized_value_uk=None, status=UNRESOLVED,
                      scope="lock_9011_finish",
                      candidate="Хром або нікель", source_refs=["catalog_2020"],
                      note="Контекст 9011: CP подписан в каталоге как никель, в других сериях как хром. Нужна проверка.")
        if re.fullmatch(code_pattern("BN"),match.group(),re.I):
            if cid in (6108,2321):
                ce["scope"]="mechanism_finish"
            else:
                ce.update(normalized_code=None, normalized_value_uk=None, status=UNRESOLVED,
                          scope="FZB_finish" if cid==920000 else "other_brand_finish",
                          candidate="Чорний нікель", note="BN подтверждён словесной расшифровкой только в механизмах. Для FZB/другой серии смысл проверить.")
        if re.fullmatch(code_pattern("NP"),match.group(),re.I):
            ce["source_refs"]=["export"]
            ce["note"]="NP (nickel) явно встречается у петли ID 8329. Уточнение о полировке не добавлялось."
        colour_events.append(ce)
    # Known escutcheon model has an attached colour suffix in a few source names.
    for suffix,code,label in [("CP","CR","Хром"),("GP","G","Золото"),("AB","AB","Антична бронза")]:
        for match in rx(r"(?<=[TТ]917)"+code_pattern(suffix)+rf"(?![{LETTERS}0-9])").finditer(name):
            colour_events.append(event(p,"color",match,code,label,scope="escutcheon_T917_suffix",
                 note="Суффикс цвета слит с известной моделью Т917; правило ограничено этой моделью.",sources=("catalog_2020",)))
    # Context-specific abbreviations not safely reusable globally.
    for match in rx(bounded(r"BL|WH|PC")).finditer(name):
        if overlap(*match.span(), blocked_colour) or overlap(*match.span(), [(e["start"],e["end"]) for e in colour_events]):
            continue
        if is_smart_product(p) or re.match(r"-\d",name[match.end():]):
            continue
        raw=match.group().upper()
        if raw == "PC" and cid == 920000 and re.search(r"\bFZB\b", name, re.I):
            ce=event(p,"color",match,"CR","Хром",scope="FZB_finish",note="CR (PC) явно встречается у FZB. Нормализация ограничена FZB.")
        elif raw == "PC":
            continue  # PC/РС is a model code outside the explicit FZB brand context.
        elif raw == "BL" and cid in (6108,2321):
            ce=event(p,"color",match,"MBP","Чорний матовий",scope="mechanism_finish",
                     note="BL (Black Mat) явно расшифрован в названиях механизмов.")
        else:
            ce=event(p,"color",match,status=UNRESOLVED,scope=colour_scope(p),candidate={"BL":"Чорний", "WH":"Білий", "PC":"Хром"}[raw],
                     note="Код вставки или покрытия требует подтверждения в этой серии; не переносить значение между контекстами.")
        colour_events.append(ce)
    # Homoglyph PC forms in FZB are genuine observed variants, kept exactly.
    for match in rx(bounded(code_pattern("PC"))).finditer(name):
        if cid == 920000 and re.search(r"\bFZB\b", name, re.I) and not overlap(*match.span(),[(e["start"],e["end"]) for e in colour_events]):
            colour_events.append(event(p,"color",match,"CR","Хром",scope="FZB_finish",note="Смешанные алфавиты PC/РС, подтверждённый контекст FZB CR (PC)."))
    # Composite finish expressions are counted separately from their components.
    ordered=sorted(colour_events,key=lambda e:e["start"])
    sequences=[]
    current=[]
    for ce in ordered:
        if current:
            gap=name[current[-1]["end"]:ce["start"]]
            if not re.fullmatch(r"\s*(?:\(UP\))?\s*[/\-]\s*",gap,re.I):
                if len(current)>1: sequences.append(current)
                current=[]
        current.append(ce)
    if len(current)>1: sequences.append(current)
    for sequence in sequences:
        known=all(e["status"]==SUPPORTED for e in sequence)
        event(p,"color_combination",(sequence[0]["start"],sequence[-1]["end"]),
              "/".join(e["normalized_code"] for e in sequence) if known else None,
              " / ".join(e["normalized_value_uk"] for e in sequence) if known else None,
              status=SUPPORTED if known else UNRESOLVED,scope="unverified_brand_or_series_finish" if any(item["scope"]=="unverified_brand_or_series_finish" for item in sequence) else colour_scope(p),
              note="Порядок компонентов сохранён. Роли наружная/внутренняя, основание/вставка не назначены. У компонентов и комбинации пересекаются product_id.",
              sources=tuple(sorted({s for e in sequence for s in e["source_refs"]})),
              candidate=" / ".join(e["normalized_value_uk"] or e["candidate"] or e["raw_value"] for e in sequence),
              components=[dict(raw_value=e["raw_value"],normalized_code=e["normalized_code"],normalized_value_uk=e["normalized_value_uk"],status=e["status"]) for e in sequence])
    for left,right in zip(ordered,ordered[1:]):
        if re.fullmatch(r"\s*\(\s*",name[left["end"]:right["start"]]) and re.match(r"\s*\)",name[right["end"]:]):
            closing=re.match(r"\s*\)",name[right["end"]:])
            same=left["status"]==right["status"]==SUPPORTED and (left["normalized_code"]==right["normalized_code"] or (left["normalized_code"],right["normalized_code"]) in (("MBP","BLACK"),("MWH","WH"),("ORB","BLACK")))
            event(p,"color_alias_expression",(left["start"],right["end"]+closing.end()),
                  left["normalized_code"] if same else None,left["normalized_value_uk"] if same else None,
                  status=SUPPORTED if same else UNRESOLVED,scope=colour_scope(p),
                  note="Альтернативные обозначения в скобках, не два разных цвета. У отдельных вариантов и целого выражения общие product_id.",
                  sources=tuple(sorted(set(left["source_refs"]+right["source_refs"]))))

    # Other text values. Longest match independently per attribute.
    for attribute in sorted({d["attribute"] for d in OTHER_RULES}):
        found=[]
        for d in [rule for rule in OTHER_RULES if rule["attribute"]==attribute]:
            if attribute=="seal_profile" and not re.search("Ущільнювач",name,re.I): continue
            if attribute=="latch_type" and not re.search(r"механізм|міжкімнатний|заскочка|межком|мех-м",name,re.I): continue
            if attribute=="construction" and d["normalized_code"]=="BUTTERFLY_HINGE" and not re.search(r"Завіса|петля",name,re.I): continue
            if attribute=="guide_type" and not re.search(r"Напрямні",name,re.I): continue
            for match in d["compiled"].finditer(name):
                if attribute=="shape" and d["normalized_code"]=="SQUARE" and re.search(r"^KEDR\s+Квадрат",name,re.I): continue
                if attribute in ("material","material_code"):
                    if overlap(*match.span(),[(e["start"],e["end"]) for e in colour_events]): continue
                    if attribute=="material_code" and match.group().lower()=="цинк" and re.search(r"білий\s+$",name[:match.start()],re.I): continue
                found.append((match,d))
        taken=[]
        for match,d in sorted(found,key=lambda item:(item[0].start(),-len(item[0].group()))):
            if overlap(*match.span(),taken): continue
            scope="general"
            if attribute=="material" and (re.search(r"ключ",name[match.end():match.end()+18],re.I) or re.search(r"ключ",name[max(0,match.start()-12):match.start()],re.I)):
                scope="key_material"
            elif attribute=="material":
                scope="mentioned_material_component_not_assigned"
            supported_event(p,d,match,scope=scope)
            taken.append(match.span())
    # Explicitly English key material; BRASS series name is not enough to assign it.
    for match in rx(bounded(r"brass\s+key")).finditer(name):
        event(p,"key_material",match,"BRASS","Латунь",scope="key_material",note="Словесное brass key; не покрытие цилиндра.")
    # Side lexicon with restricted contexts.
    side_context = cid in ENTRY or bool(re.search(r"Завіса|петля|ручк|T5D|китай\.\s*двері",name,re.I))
    if side_context:
        ambiguous=rx(r"лів,\s*прав").search(name)
        if ambiguous:
            event(p,"side",ambiguous,status=UNRESOLVED,note="В одной записи перечислены обе стороны, выбрать одну нельзя.")
        for match in rx(bounded(r"L|R|ліва|права|лів\.|прав\.|універсальний|універсальна|УНИВЕРСАЛЬНА")).finditer(name):
            if ambiguous and overlap(*match.span(),[ambiguous.span()]):continue
            raw=match.group().casefold()
            code="L" if raw in ("l","ліва","лів.") else "R" if raw in ("r","права","прав.") else "UNIVERSAL"
            event(p,"side",match,code,{"L":"Ліва","R":"Права","UNIVERSAL":"Універсальна"}[code])
    # Cylinder composite codes remain intact. No unsupported suffix decomposition.
    if cid in CYLINDER or re.search(r"циліндр|цилиндр",name,re.I):
        for match in rx(rf"(?<![{LETTERS}])Z[СC]?(?:AB|BM|N|G)?(?![{LETTERS}0-9])").finditer(name):
            event(p,"cylinder_configuration_code",match,status=UNRESOLVED,scope="mechanical_cylinder",
                  note="Слитный код конфигурации/покрытия. Полная расшифровка владельцем ещё не подтверждена.")
        for match in rx(r"(?<=\d)[TТ](?=\s*[*\/)]|$)").finditer(name):
            event(p,"cylinder_turn_marker",match,status=UNRESOLVED,scope="mechanical_cylinder",candidate="Поворотник",
                  note="Наличие и положение T/Т записаны буквально; семантика не подтверждена.")
    if is_smart_product(p):
        for match in rx(r"(?<=L4-)(?:PB|SB|SR)").finditer(name):
            event(p,"smart_variant_code",match,status=UNRESOLVED,scope="SMART_L4",
                  note="PB/SB/SR внутри L4 могут обозначать функциональный вариант, а не цвет. Цветовой словарь не применялся.")
    # Bearings and series markers.
    if re.search(r"Завіса|петл",name,re.I):
        for match in rx(bounded(r"B4|B2|2BB")).finditer(name):
            is_four=match.group().upper()=="B4"
            event(p,"bearing_designation",match,"4" if is_four else None,"4 підшипники" if is_four else None,
                  status=SUPPORTED if is_four else UNRESOLVED, sources=("user",) if is_four else ("export",),
                  candidate=None if is_four else "2 підшипники", note="B4 подтверждено пользователем." if is_four else "B2/2BB требуют подтверждения значения перед разметкой.")
    for match in rx(bounded(r"G(?=\s+(?:410[BC]-S|100\s+заскочка))")).finditer(name):
        event(p,"series_marker",match,status=UNRESOLVED,note="G перед моделью механизма/защёлки. Не цвет G=золото.")
    for match in rx(r"\(UP\)").finditer(name):
        event(p,"position_marker",match,status=UNRESOLVED,note="Цветовая комбинация известна, но пространственный смысл UP не подтвержден.")
    # Model-like short codes deliberately not interpreted as colours.
    for pattern,scope in [(r"\bHG\s*\d+\s+(L)\b","padlock_shackle_code"),(r"\bLXCP\s*-\s*\d+\s*-\s*(L)\b","padlock_variant"),(r"\b1082\s+(L)\b","lever_lock_variant")]:
        for match in rx(pattern).finditer(name):
            event(p,"unresolved_marker",match.span(1),status=UNRESOLVED,scope=scope,
                  note="Отдельная L в этой модели не доказана как сторона.")
    # Model prefixes ET/BK and embedded WC are inventoried without guessing their functions.
    accessory_codes="(?:"+"|".join(code_pattern(s) for s in ("ET","BK","WC"))+")"
    for match in rx(rf"(?<![{LETTERS}]){accessory_codes}(?=\s*[-\d]|\b)|(?<=D672)(?:BK|ET)|(?<=D80-23)(?:BK|ET)").finditer(name):
        if any(e["attribute"]=="function_code" and e["start"]==match.start() for e in EVENTS[own_start:]):continue
        event(p,"accessory_function_code",match,status=UNRESOLVED,
              note="Код внутри модели/серии аксессуара, значение функции требует подтверждения. Не является цветом.")
    for match in rx(r"72/62|69/52").finditer(name):
        event(p,"unresolved_size_code",match,status=UNRESOLVED,
              note="Код геометрии/совместимости пока не расшифрован; числовые оси не назначались.")
    # Unit dictionaries record just the literal unit following a number, not measurements.
    unit_values={"mm":("MM","мм"),"мм":("MM","мм"),"м":("M","м"),"m":("M","м"),"см":("CM","см"),
                 "кг":("KG","кг"),"kg":("KG","кг"),"н":("N","Н"),"шт.":("PCS","Штука"),"шт":("PCS","Штука"),
                 "пар.":("PAIR","Пара"),"компл.":("SET","Комплект"),"кл.":("KEY","Ключ"),"кл":("KEY","Ключ"),
                 "ключів":("KEY","Ключ"),"ключiв":("KEY","Ключ"),"штук":("PCS","Штука"),"градусів":("DEG","Градус"),"градусiв":("DEG","Градус")}
    for match in rx(r"(?<=\d)\s*(mm|мм|см|м|m|кг|kg|Н|шт\.?|штук|пар\.|компл\.|кл\.?|ключ[іi]в|градус[іi]в)(?![A-Za-zА-Яа-яІіЇїЄєҐґ])").finditer(name):
        code,label=unit_values[match.group(1).lower()]
        event(p,"measurement_unit",match.span(1),code,label,scope="literal_unit_after_number",
              note="Единица измерения/счёта из названия. Назначение самого числа не выводилось.")
    # First explicit product-type mention, rather than all component nouns in a title.
    type_matches=[]
    for code,label,pattern in PRODUCT_TYPE_RULES:
        for match in rx(bounded(pattern)).finditer(name):
            type_matches.append((match.start(),-len(match.group()),match,code,label))
    if re.search(r"Ущільнювач\s+Стоєва",name,re.I):
        match=rx(r"Ущільнювач\s+Стоєва").search(name)
        event(p,"product_type",match,status=UNRESOLVED,scope="ambiguous_title_type",candidate="Стоєва / ущільнювач",
              note="В названии вместе УЩІЛЬНЮВАЧ и Стоєва. Не выбирать физический тип только по первому слову.")
    elif type_matches:
        _,_,match,code,label=min(type_matches,key=lambda item:(item[0],item[1]))
        event(p,"product_type",match,code,label,scope="first_explicit_type_mention",
              note="Первое явное название вида изделия; это словарь типов, категории товаров не изменены.")
    return EVENTS[own_start:]


def load_rows(path, table_name):
    value=json.loads(path.read_text(encoding="utf-8-sig"))
    if isinstance(value,list): rows=value
    elif isinstance(value,dict):
        arrays=[rows for key,rows in value.items() if isinstance(rows,list) and table_name.lower() in key.lower()]
        if not arrays: arrays=[rows for rows in value.values() if isinstance(rows,list)]
        if len(arrays)!=1: raise ValueError(f"{path}: cannot identify one {table_name} array")
        rows=arrays[0]
    else: raise ValueError(f"{path}: expected a JSON array or export wrapper")
    if not all(isinstance(row,dict) for row in rows):raise ValueError(f"{path}: array must contain objects")
    return rows


def number(text):
    value=Decimal(text.replace(",","."))
    return int(value) if value==value.to_integral() else float(value)


def format_number(value):
    text=format(Decimal(str(value)),"f")
    return text.rstrip("0").rstrip(".") if "." in text else text


def source_hash(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


class Categories:
    def __init__(self,rows):
        self.rows=rows
        self.by_id={row["Id"]:row for row in rows}
        if len(self.by_id)!=len(rows):raise ValueError("Duplicate category IDs")
        self.paths={}
        self.issues={}
        for cid in self.by_id:
            labels=[]; seen=set(); target=cid; issues=[]
            while target is not None:
                if target in seen:
                    labels.append("[цикл категорій]");issues.append("category_cycle");break
                seen.add(target)
                category=self.by_id.get(target)
                if category is None:
                    labels.append(f"[невідома категорія {target}]");issues.append("category_parent_missing");break
                labels.append(category.get("ShortNameUk") or category.get("Name") or str(target))
                target=category.get("ParentId")
            self.paths[cid]=" > ".join(labels[::-1])
            self.issues[cid]=issues


class Dictionary:
    def __init__(self,path,categories):
        global COLOR_RULES,OTHER_RULES,PRODUCT_TYPE_RULES,ENTRY,INTERIOR,CYLINDER,SMART_CATEGORIES
        self.data=json.loads(path.read_text(encoding="utf-8-sig"))
        if self.data.get("approval_status")!="supported_mappings_approved_unresolved_remain_null":
            raise ValueError("Dictionary has no stage 4 approval status")
        self.config=self.data["parser_configuration"]
        self.entries={}
        self.folded_entries={}
        self.allowed=set()
        for entry in self.data["entries"]:
            key=(entry["attribute"],entry["scope"],entry["raw_value"])
            if key in self.entries:
                old=self.entries[key]
                if (old["status"],old["normalized_code"],old["normalized_value_uk"])!=(entry["status"],entry["normalized_code"],entry["normalized_value_uk"]):
                    raise ValueError(f"Conflicting dictionary mapping: {key}")
            self.entries[key]=entry
            self.folded_entries[(key[0],key[1],folded(key[2]))]=entry
            if entry["status"]==SUPPORTED:
                if entry["normalized_code"] is None or entry["normalized_value_uk"] is None:raise ValueError(f"Approved null: {key}")
                self.allowed.add((entry["attribute"],entry["normalized_code"],entry["normalized_value_uk"]))
        compiled=[]
        for rule in self.data["extraction_rules"]:
            item=dict(rule)
            item["compiled"]=rx(bounded(item["pattern"]))
            compiled.append(item)
        COLOR_RULES=[rule for rule in compiled if rule["attribute"]=="color"]
        OTHER_RULES=[rule for rule in compiled if rule["attribute"]!="color"]
        PRODUCT_TYPE_RULES=self.config["product_type_rules"]
        ENTRY=set(self.config["entry_category_ids"])
        INTERIOR=set(self.config["interior_category_ids"])
        CYLINDER=set(self.config["cylinder_category_ids"])
        SMART_CATEGORIES=set(self.config["smart_category_ids"])
        for cid,path_text in categories.paths.items():
            if "Вхідні двері" in path_text:ENTRY.add(cid)
            if "Міжкімнатні двері" in path_text or path_text.endswith(" > Виставка"):INTERIOR.add(cid)
            if "Циліндри" in path_text:
                if "SMART" in path_text:SMART_CATEGORIES.add(cid)
                else:CYLINDER.add(cid)

    def recognize(self,p):
        EVENTS.clear()
        observations=extract(p)
        for observation in observations:
            key=(observation["attribute"],observation["scope"],observation["raw_value"])
            entry=self.entries.get(key) or self.folded_entries.get((key[0],key[1],folded(key[2])))
            if entry is not None and entry.get("eligible_name_pattern") and not re.search(entry["eligible_name_pattern"],p.get("Name") or "",re.I):
                entry=None
            if entry is not None:
                observation.update(status=entry["status"],normalized_code=entry["normalized_code"],
                    normalized_value_uk=entry["normalized_value_uk"],note=entry.get("note",observation.get("note","")),
                    source_refs=entry.get("source_refs",observation.get("source_refs",[])),
                    selected_by=entry.get("selected_by"))
            elif observation["status"]==SUPPORTED:
                allowed=(observation["attribute"],observation["normalized_code"],observation["normalized_value_uk"]) in self.allowed
                if observation["attribute"]=="color_combination" and self.config.get("allow_known_component_combinations"):
                    allowed=all(component["status"]==SUPPORTED for component in observation.get("components") or [])
                if not allowed:
                    observation.update(status=UNRESOLVED,normalized_code=None,normalized_value_uk=None,
                        note="Новое значение не имеет утверждённого соответствия в словаре.")
        # Atomic mappings may have changed since the original composite was inventoried.
        # Rebuild components from their positions, preserving unknown components and source order.
        for observation in observations:
            if observation["attribute"]!="color_combination":continue
            atoms=sorted((item for item in observations if item["attribute"]=="color" and
                observation["start"]<=item["start"] and item["end"]<=observation["end"]),key=lambda item:item["start"])
            if len(atoms)<2:continue
            observation["components"]=[{key:item[key] for key in ("raw_value","normalized_code","normalized_value_uk","status")} for item in atoms]
            known=all(item["status"]==SUPPORTED for item in atoms)
            observation.update(status=SUPPORTED if known else UNRESOLVED,
                normalized_code="/".join(item["normalized_code"] for item in atoms) if known else None,
                normalized_value_uk=" / ".join(item["normalized_value_uk"] for item in atoms) if known else None)
        return observations


def product_kind(p):
    name=p.get("Name") or ""
    cid=p.get("CategoryId")
    if re.match(r"(?:Робота|Ремонт|Доставка)\b",name,re.I):return "service"
    if cid in (6799,8719) or re.match(r"(?:Каталог|Буклет|Образцы|Рекламні)\b",name,re.I):return "advertising"
    if re.search(r"Дверне\s+полотно|Каркасно-щитові\s+двері",name,re.I):return "door_leaf"
    if cid in ENTRY and re.match(r"Двері\b",name,re.I):return "entrance_door"
    if re.search(r"Коробка|Лиштва|Добірна\s+дошка|Розширювач|Плінтус|профіль|Притворна\s+планка|Капітель|Стоєва|ПОРТАЛ",name,re.I):return "trim"
    if re.search(r"Завіса|петля",name,re.I):
        if re.search(r"прихован",name,re.I):return "concealed_hinge"
        if re.search(r"приварна|ввертна|проточена|точена",name,re.I):return "round_hinge"
        if cid in (6139,1707) or "FLUSH" in name.upper() or re.search(r"(?:75|100|125)\s*[*xх×]\s*(?:62|63|75)\b",name,re.I):return "flat_hinge"
        return "other_hinge"
    if re.search(r"ковпачок|\bCAP\b",name,re.I):return "hinge_cap"
    if is_smart_product(p):return "smart_cylinder"
    if re.search(r"циліндр|цилиндр|секрет",name,re.I):return "cylinder"
    if re.search(r"вічко",name,re.I):return "viewer"
    if re.search(r"дотягувач",name,re.I):return "closer"
    if re.search(r"Ущільнювач",name,re.I):return "seal"
    if re.match(r"KEDR\s+Квадрат\b",name,re.I):return "spindle"
    if re.search(r"ґвинт|гвинт|саморіз",name,re.I):return "screw"
    if re.search(r"рейка",name,re.I):return "rail"
    if re.search(r"шпінгалет",name,re.I):return "bolt"
    if re.search(r"ручка|ручки",name,re.I):return "handle"
    if re.search(r"замок|Корпус\s+замка",name,re.I):return "lock"
    if re.search(r"Кутник|Пластина|Провушина|Підвіс|Кріпленння|Стріла",name,re.I):return "metal_fitting"
    return "other"


class Extraction:
    def __init__(self,p,dictionary,categories):
        self.p=p;self.name=p.get("Name");self.cid=p.get("CategoryId")
        self.dictionary=dictionary;self.categories=categories
        self.kind=product_kind(p)
        self.reasons={}
        self.information={}
        self.conflicted_fields=set()
        self.remove=[]
        self.extra={"product_kind":self.kind}
        self.row={key:None for key in COLUMNS}
        self.row.update(product_id=p["Id"],category_id=self.cid,
            category_path=categories.paths.get(self.cid),original_name=self.name)

    def issue(self,code,text):
        self.reasons.setdefault(code,[])
        if text not in self.reasons[code]:self.reasons[code].append(text)

    def inform(self,code,text):
        self.information.setdefault(code,[])
        if text not in self.information[code]:self.information[code].append(text)

    def drop(self,start,end):
        if 0<=start<end<=len(self.name):self.remove.append((start,end))

    def field(self,key,value):
        if key in self.conflicted_fields:return
        if self.row[key] is not None and self.row[key]!=value:
            self.row[key]=None
            self.conflicted_fields.add(key)
            self.issue("conflicting_"+key,f"Несколько разных значений для {key}; итог оставлен null.")
        else:self.row[key]=value

    def lexical(self):
        self.observations=self.dictionary.recognize(self.p)
        attributes=defaultdict(dict)
        unresolved=[]
        remove_attributes={"color","color_combination","color_alias_expression","side","size_variant","branding","packaging","bearing_designation","door_infill"}
        for observation in self.observations:
            attribute=observation["attribute"]
            if observation["status"]==UNRESOLVED:
                unresolved.append(dict(attribute=attribute,raw=observation["raw_value"],code=None,name_uk=None,
                    reason=observation.get("note") or "Неоднозначное значение"))
                self.issue("unresolved_"+attribute,f"«{observation['raw_value']}»: {observation.get('note') or 'соответствие не подтверждено'}")
            elif attribute not in ("color","color_combination","color_alias_expression","measurement_unit","side"):
                key=(observation["normalized_code"],observation["normalized_value_uk"],observation["scope"])
                if key not in attributes[attribute]:
                    attributes[attribute][key]=dict(code=key[0],name_uk=key[1],scope=key[2],raw=[])
                if observation["raw_value"] not in attributes[attribute][key]["raw"]:
                    attributes[attribute][key]["raw"].append(observation["raw_value"])
            if attribute in remove_attributes:
                # Keep unknown model markers. Identified colour roles may be removed even if their decoding is unknown.
                if observation["status"]==SUPPORTED or attribute in ("color","color_combination","color_alias_expression","bearing_designation","side"):
                    start,end=observation["start"],observation["end"]
                    prefix=re.match(r"Super\s+PET\s+",observation["raw_value"],re.I)
                    if prefix:start+=prefix.end()
                    self.drop(start,end)
        if attributes:self.extra["attributes"]={key:list(values.values()) for key,values in sorted(attributes.items())}
        if unresolved:self.extra["unresolved_attributes"]=unresolved
        if any(e["attribute"]=="branding" for e in self.observations) and re.search(r"\bKEDR\b|\bCLASS\b",self.name,re.I):
            self.issue("brand_marking_conflict","В названии одновременно указан бренд и No Name; исходные сведения сохранены без выбора одного варианта.")
        # Side remains null unless explicitly stated and unambiguous.
        side_events=[e for e in self.observations if e["attribute"]=="side"]
        known={e["normalized_code"] for e in side_events if e["status"]==SUPPORTED}
        if any(e["status"]==UNRESOLVED for e in side_events) or len(known)>1:
            self.issue("ambiguous_side","Сторона неоднозначна; side=null.")
        elif len(known)==1:self.row["side"]=next(iter(known))
        # Resolved bearing counts are separated from the literal designation.
        bearing_events=[e for e in self.observations if e["attribute"]=="bearing_designation"]
        if bearing_events:
            counts={int(e["normalized_code"]) for e in bearing_events if e["status"]==SUPPORTED and str(e["normalized_code"]).isdigit()}
            self.extra["bearing_count"]=next(iter(counts)) if len(counts)==1 and all(e["status"]==SUPPORTED for e in bearing_events) else None
        if self.kind in ("flat_hinge","other_hinge"):
            for match in re.finditer(bounded(r"B\d+|\d+BB"),self.name,re.I):
                if not any(e["attribute"]=="bearing_designation" and e["start"]==match.start() for e in self.observations):
                    self.extra["bearing_count"]=None
                    self.issue("unknown_bearing_designation",f"Неизвестное обозначение подшипников «{match.group()}».")
            # A code in the explicit finish slot of a flat hinge is a found value,
            # even when it is new to the dictionary. Absence of that slot is harmless.
            if self.kind=="flat_hinge":
                slot=re.search(rf"{NUMBER}\s*[*xх×]\s*{NUMBER}(?:\s*[*xх×]\s*{NUMBER})?(?:mm|мм)?(?:\s*-\s*(?:B\d+|\d+BB))?\s*-\s*(?P<code>[A-Za-z]{{2,20}})(?![A-Za-z0-9])",self.name,re.I)
                if slot and not overlap(*slot.span("code"),[(item["start"],item["end"]) for item in self.observations]):
                    raw=slot.group("code")
                    self.extra.setdefault("unresolved_attributes",[]).append(dict(attribute="color",raw=raw,code=None,name_uk=None,reason="Новый код в позиции цвета плоской петли."))
                    self.row["color_raw"]=raw
                    self.drop(*slot.span("code"))
                    self.issue("unresolved_color",f"Неизвестный код цвета «{raw}» в позиции после размера петли.")
        # Opening method descriptions form one variant clause of L4 smart cylinders.
        if self.kind=="smart_cylinder":
            clause=re.search(r"\s+з\s+(?:паролем|відкриттям)\b",self.name,re.I)
            if clause and any(e["attribute"]=="opening_method" for e in self.observations):self.drop(clause.start(),len(self.name))

    def colors(self):
        values=[e for e in self.observations if e["attribute"]=="color"]
        compounds=[e for e in self.observations if e["attribute"] in ("color_combination","color_alias_expression")]
        if not values:return
        start=min(e["start"] for e in values+compounds)
        end=max(e["end"] for e in values+compounds)
        self.row["color_raw"]=self.name[start:end]
        self.extra["color_mentions"]=[dict(raw=e["raw_value"],code=e["normalized_code"],name_uk=e["normalized_value_uk"],status=e["status"],
            source_refs=e.get("source_refs",[]),selected_by=e.get("selected_by")) for e in values]
        combinations=[e for e in compounds if e["attribute"]=="color_combination"]
        if len(combinations)==1:
            chosen=combinations[0]
            covered=all(chosen["start"]<=e["start"] and e["end"]<=chosen["end"] for e in values)
            if covered:
                components=chosen["components"]
                self.extra["color_components"]=components
                for index,(code_key,name_key) in enumerate((("color_code","color_name"),("color_secondary_code","color_secondary_name"))):
                    if index<len(components) and components[index]["status"]==SUPPORTED:
                        self.row[code_key]=components[index]["normalized_code"]
                        self.row[name_key]=components[index]["normalized_value_uk"]
                self.extra["color_order_policy"]="Основной — первый компонент в названии; второй — следующий. Физическая роль сторон/вставок не предполагается."
                if len(components)>2:self.extra["additional_color_components"]=components[2:]
                return
        if any(e["status"]!=SUPPORTED for e in values+compounds):return
        codes={(e["normalized_code"],e["normalized_value_uk"]) for e in values}
        if len(codes)==1:
            code,label=next(iter(codes))
            self.row.update(color_code=code,color_name=label)
        else:
            # MBP (чорний) is a compatible general description of a more specific finish.
            aliases=[e for e in compounds if e["attribute"]=="color_alias_expression" and e["start"]<=start and end<=e["end"]]
            if len(aliases)==1:self.row.update(color_code=aliases[0]["normalized_code"],color_name=aliases[0]["normalized_value_uk"])
            else:self.issue("multiple_unassigned_colors","Несколько цветов без однозначной связи; color_code/name=null.")

    def measurements(self):
        name=self.name
        measurements=[]; occupied=[]
        unit_factors=self.dictionary.config["unit_factors_to_mm"]
        def mm(value,unit):
            if unit is None or unit.lower() not in unit_factors:return None
            return number(str(Decimal(str(value))*Decimal(str(unit_factors[unit.lower()]))))
        linear_roles={"center_distance","plate_size","key_length","rod_length","thickness","explicit_length","explicit_width","diameter","wing_dimensions","dimensions","section","cylinder_parts","diameter_tuple","roll_length","rail_length","bolt_length","screw_length","nominal_size","viewer_range"}
        def add(match,role,values,unit=None,known_span=None):
            start,end=known_span or match.span()
            implicit=unit is None and role in linear_roles
            if implicit:unit=self.dictionary.config["default_dimension_unit"]
            measurement=dict(role=role,raw=name[start:end],values=values,unit=unit,
                values_mm=[mm(value,unit) for value in values] if unit and unit.lower() in unit_factors else None,start=start,end=end)
            if implicit:measurement["unit_basis"]="user_default_mm_when_omitted"
            measurements.append(measurement);occupied.append((start,end))
            self.drop(start,end)
            return measurement
        # Functional quantities are parsed before geometric dimensions.
        patterns=[
            ("center_distance",rf"(?:межось\.?|міжось\.?|межосьова|міжосьова)\s*(?:відстань\s*)?(?P<v>{NUMBER})\s*(?P<u>мм|mm)?"),
            ("plate_size",rf"планка\s*(?P<v>{NUMBER})(?:\s*[*xх×]\s*(?P<v2>{NUMBER}))?\s*(?P<u>мм|mm)"),
            ("key_length",rf"ключ\s+(?P<v>{NUMBER})\s*(?P<u>мм|mm)"),
            ("rod_length",rf"шток\s+(?P<v>{NUMBER})\s*(?P<u>мм|mm)"),
            ("thickness",rf"(?:товщина|толщина|товщ\.)\s*\(?\s*(?P<v>{NUMBER})(?:\s*[-–]\s*(?P<v2>{NUMBER}))?\s*(?P<u>мм|mm)?"),
            ("explicit_length",rf"(?:довжина|довж\.|длина|дл\.)\s*(?P<v>{NUMBER})(?:\s*[-–]\s*(?P<v2>{NUMBER}))?\s*(?P<u>мм|mm|см|cm|м|m)?"),
            ("explicit_width",rf"(?:шир\.|ширина)\s*(?P<v>{NUMBER})\s*(?P<u>мм|mm|см|cm)?"),
            ("diameter",rf"(?:діаметр|диаметр|Ф|[dD])\s*(?P<v>{NUMBER})\s*(?P<u>мм|mm)?"),
            ("weight",rf"(?P<v>{NUMBER})(?:\s*[-–]\s*(?P<v2>{NUMBER}))?\s*(?P<u>кг|kg)"),
            ("force",rf"(?P<v>{NUMBER})\s*(?P<u>Н|N)(?![{LETTERS}])"),
            ("keys",rf"(?P<v>\d+)\s*(?:кл\.?|ключ[іi]в|ключей)(?![{LETTERS}])"),
            ("angle",rf"(?P<v>{NUMBER})\s*(?:градус[іi]в|градусів|°)"),
        ]
        for role,pattern in patterns:
            for match in re.finditer(pattern,name,re.I):
                if overlap(*match.span(),occupied):continue
                if match.end()<len(name) and re.match(rf"[{LETTERS}0-9]",name[match.end()]):continue
                values=[number(match.group("v"))]
                if "v2" in match.groupdict() and match.group("v2"):values.append(number(match.group("v2")))
                unit=match.groupdict().get("u")
                # d/D/Ф are only diameter markers when not inside an alphanumeric model.
                if role=="diameter" and match.start()>0 and re.match(rf"[{LETTERS}0-9]",name[match.start()-1]):continue
                if role=="diameter" and re.search(r"^[dD]",match.group()) and self.kind not in ("round_hinge","hinge_cap","other_hinge","metal_fitting"):continue
                measure=add(match,role,values,unit)
                if role=="center_distance":self.extra["center_distance_mm"]=measure["values_mm"][0]
                elif role=="plate_size":self.extra["plate_dimensions_mm"]=measure["values_mm"]
                elif role=="key_length":self.extra["key_length_mm"]=measure["values_mm"][0]
                elif role=="rod_length":self.extra["rod_length_mm"]=measure["values_mm"][0]
                elif role=="thickness":
                    if len(values)==1:self.field("size_thickness",measure["values_mm"][0])
                    else:
                        self.extra["thickness_range_mm"]={"min":measure["values_mm"][0],"max":measure["values_mm"][1]}
                        self.inform("thickness_is_range","Толщина указана диапазоном; size_thickness=null, диапазон сохранён отдельно.")
                elif role=="explicit_length":
                    if len(values)==1:self.field("size_length",measure["values_mm"][0])
                    else:
                        self.extra["length_range_mm"]={"min":measure["values_mm"][0],"max":measure["values_mm"][1]}
                        self.inform("length_is_range","Длина указана диапазоном; единственное значение не выбиралось.")
                elif role=="explicit_width":self.field("size_width",measure["values_mm"][0])
                elif role=="diameter":
                    self.field("size_diameter",measure["values_mm"][0])
                    self.extra["diameter_mm"]=self.row["size_diameter"]
                elif role=="weight":
                    key="door_weight_kg" if self.kind=="closer" else "load_kg"
                    self.extra[key]=dict(min=values[0],max=values[-1]) if len(values)>1 else values[0]
                    if self.kind=="concealed_hinge":self.extra["load_applies_to"]="Набір завіс, кількість з назви"
                elif role=="force":self.extra["force_N"]=values[0]
                elif role=="keys":self.extra["key_count"]=values[0]
                elif role=="angle":self.extra["angle_degrees"]=values[0]
        for match in re.finditer(rf"(?:крило|крилом)\s*\(?\s*(?P<v>{NUMBER})(?:\s*/\s*(?P<v2>{NUMBER}))?\s*(?P<u>мм|mm)?",name,re.I):
            if overlap(*match.span(),occupied):continue
            values=[number(match.group("v"))]
            if match.group("v2"):values.append(number(match.group("v2")))
            measure=add(match,"wing_dimensions",values,match.group("u"))
            value_end=match.end("u") if match.group("u") else match.end("v2") if match.group("v2") else match.end("v")
            self.remove[-1]=(match.start("v"),value_end)
            self.extra["wing_dimensions"]=dict(ordered_values=values,ordered_mm=measure["values_mm"],unit=measure["unit"])
            if len(values)>1:self.issue("wing_dimensions_unconfirmed","У крыла несколько значений; их назначение не подтверждено.")
        # Compound dimensions: * / x / Cyrillic х / ×, with optional cylinder T markers.
        tuple_pattern=rf"(?<![{LETTERS}0-9])(?P<a>{NUMBER})(?P<ta>[TТ]?)\s*[*xх×]\s*(?P<b>{NUMBER})(?P<tb>[TТ]?)(?:\s*[*xх×]\s*(?P<c>{NUMBER}))?\s*(?P<u>мм|mm|см|cm)?"
        for match in re.finditer(tuple_pattern,name,re.I):
            if overlap(*match.span(),occupied):continue
            start,end=match.span()
            if re.search(r"№\s*$",name[max(0,start-4):start]):continue
            bad_unit=re.match(r"\s*(?:шт|уп|ящ|пар|компл)",name[end:],re.I)
            if bad_unit:
                self.extra.setdefault("invalid_dimensions",[]).append(dict(raw=name[start:end+bad_unit.end()],normalized_value=None))
                self.issue("invalid_dimension_unit","В размерном выражении указана единица количества вместо линейной; значение не исправлялось.")
                continue
            values=[number(match.group("a")),number(match.group("b"))]
            if match.group("c"):values.append(number(match.group("c")))
            role="cylinder_parts" if self.kind=="cylinder" else "section" if self.kind=="seal" else "dimensions"
            unit=match.group("u")
            measure=add(match,role,values,unit)
            if match.group("ta") or match.group("tb"):
                measure["part_markers"]=[match.group("ta") or None,match.group("tb") or None]
            if self.kind=="flat_hinge":
                measure["axes"]=["length","width","thickness"][:len(values)]
                converted=measure["values_mm"]
                if converted:
                    self.field("size_length",converted[0]);self.field("size_width",converted[1])
                    if len(converted)==3:self.field("size_thickness",converted[2])
            elif role=="cylinder_parts":
                self.extra["cylinder_parts"]=dict(ordered_raw_values=values,ordered_mm=measure["values_mm"],turn_markers=measure.get("part_markers"))
                # Ordered halves are explicit dimensions, not an ambiguous axis assignment.
                measure["axes"]=["part_a","part_b"][:len(values)]
            elif role=="section":
                self.extra["section_dimensions"]=dict(ordered_values=values,ordered_mm=measure["values_mm"])
                self.issue("section_axes_unconfirmed","Извлечено сечение уплотнителя; назначение двух осей не подтверждено.")
            else:
                self.issue("dimension_axes_unconfirmed","Размерное выражение извлечено, порядок длины/ширины/толщины не подтверждён.")
        # Diameter tuple D14*46: the preceding diameter has been captured separately.
        if self.kind in ("round_hinge","hinge_cap"):
            for match in re.finditer(rf"(?:D|d|Ф)\s*{NUMBER}\s*[*xх×]\s*(?P<v>{NUMBER})(?:\s*(?P<u>мм|mm))?",name,re.I):
                if any(m["role"]=="diameter_tuple" for m in measurements):continue
                values=[number(text) for text in re.findall(NUMBER,match.group())][:2]
                measure=add(match,"diameter_tuple",values,match.groupdict().get("u"))
                measure["axes"]=["diameter","length"]
                self.field("size_diameter",measure["values_mm"][0]);self.field("size_length",measure["values_mm"][1])
        # Single measurements with explicit units, excluding functional quantities already claimed.
        for match in re.finditer(rf"(?<![{LETTERS}0-9])(?P<v>{NUMBER})(?:\s*[-–]\s*(?P<v2>{NUMBER}))?\s*(?P<u>мм|mm|см|cm|м|m)(?![{LETTERS}0-9])",name,re.I):
            if overlap(*match.span(),occupied):continue
            values=[number(match.group("v"))]
            if match.group("v2"):values.append(number(match.group("v2")))
            unit=match.group("u")
            role="roll_length" if self.kind=="seal" and unit.lower() in ("м","m") else "rail_length" if self.kind=="rail" else "nominal_size"
            if self.kind=="bolt" and re.search(r"торцевий\s+"+re.escape(match.group()),name,re.I):role="bolt_length"
            if self.kind=="screw" and re.search(r"[MМ]\d+",name):role="screw_length"
            measure=add(match,role,values,unit)
            if role=="roll_length":self.extra["roll_length_m"]=values[0]
            elif role in ("rail_length","bolt_length","screw_length") and len(values)==1:self.field("size_length",measure["values_mm"][0])
            else:self.issue("single_dimension_role_unconfirmed","Линейный размер найден, но измеряемая часть/ось не подтверждена.")
        if self.kind=="viewer":
            match=re.search(r"(?<!\d)(\d+)\s*[-–]\s*(\d+)(?!\d)",name)
            if match:
                add(match,"viewer_range",[int(match.group(1)),int(match.group(2))])
                self.extra["viewer_range"]=dict(raw=match.group(),min=int(match.group(1)),max=int(match.group(2)),meaning=None,unit="mm")
                self.issue("viewer_range_unconfirmed","Диапазон вічка сохранён в мм; его назначение не подтверждено.")
        if self.kind=="screw":
            match=re.search(r"[MМ](\d+)",name)
            if match:self.extra["thread_designation"]="M"+match.group(1);self.drop(*match.span())
        quantities=[]
        for match in re.finditer(rf"(?<![{LETTERS}0-9])(?P<v>{NUMBER})\s*(?P<u>штука|штук|шт\.?|пар\.|компл\.?)(?![{LETTERS}])",name,re.I):
            if any(match.start()<b and a<match.end() for a,b in occupied):continue
            if re.search(r"[*xх×]\s*$",name[:match.start()]):continue
            raw_unit=match.group("u").lower()
            unit="PAIR" if raw_unit.startswith("пар") else "SET" if raw_unit.startswith("компл") else "PCS"
            quantities.append(dict(value=number(match.group("v")),unit=unit,raw=match.group()))
            self.drop(*match.span())
        if quantities:self.extra["quantities_from_name"]=quantities
        # Explicit hinge counts and catalogue editions are not dimensions.
        match=re.search(r"(\d+)\s*петл",name,re.I)
        if match:
            self.extra["hinges_in_load_spec"]=int(match.group(1))
            if self.kind=="concealed_hinge":
                clause=re.search(rf"\(\s*{NUMBER}\s*(?:кг|kg)\s*[xх×]\s*\d+\s*петл\s*\)",name,re.I)
                if clause:self.drop(*clause.span())
        if self.kind=="advertising" and re.search(r"Каталог",name,re.I):
            match=re.search(r"\b(20\d{2})\b",name)
            if match:self.extra["edition_year"]=int(match.group(1));self.drop(*match.span())
        # A separate scalar component does not invalidate an unambiguous body tuple.
        # Its role can still need review; its raw value is retained in measurements.
        primary=[m for m in measurements if m["role"] in ("dimensions","section","cylinder_parts","diameter_tuple")]
        if not primary:primary=[m for m in measurements if m["role"] in ("nominal_size","bolt_length","screw_length","rail_length")]
        if len(primary)==1:
            selected=primary[0]
            marker_values=selected.get("part_markers")
            components=[format_number(value)+(marker_values[n] if marker_values and n<len(marker_values) and marker_values[n] else "") for n,value in enumerate(selected["values"])]
            self.row["size"]="×".join(components)+(f" {selected['unit'].lower()}" if selected["unit"] else "")
        elif len(primary)>1:
            self.issue("multiple_size_expressions","Несколько размерных выражений: размер сохранён в measurements, size и спорные оси оставлены null.")
            self.row.update(size=None,size_length=None,size_width=None)
        else:
            body=[m for m in measurements if m["role"] in ("explicit_length","explicit_width","thickness","diameter")]
            if not self.extra.get("invalid_dimensions") and body:
                labels={"explicit_length":"довжина","explicit_width":"ширина","thickness":"товщина","diameter":"діаметр"}
                self.row["size"]="; ".join(labels[m["role"]]+" "+"–".join(format_number(value) for value in m["values"])+" "+m["unit"].lower() for m in body)
        if measurements:
            self.extra["measurements"]=[{key:value for key,value in m.items() if key not in ("start","end")} for m in measurements]
        if self.kind=="flat_hinge":
            if not any(e["attribute"]=="bearing_designation" for e in self.observations):
                self.extra["bearing_count"]=None

    def packaging(self):
        stack=[];pairs=[]
        for index,char in enumerate(self.name):
            if char=="(":stack.append(index)
            elif char==")" and stack:
                start=stack.pop()
                if not stack:pairs.append((start,index+1))
        if stack or self.name.count("(")!=self.name.count(")"):
            self.inform("unbalanced_parentheses","В исходном названии несбалансированные скобки; original_name сохранён без исправления.")
        packages=[]
        for start,end in pairs:
            raw=self.name[start:end]
            if re.search(r"шт|уп\.?|ящ|пар\.|пара|компл",raw,re.I):
                remainder=re.sub(r"штука|штук|шт|упаковка|упаковки|уп\.?|ящик|ящ\.?|пачка|мішок|пара|пар\.|комплект|компл\.|рулон", "",raw,flags=re.I)
                remainder=re.sub(r"[\d\s(),.*xх×/\\\-]+","",remainder)
                if not re.search(rf"[{LETTERS}]",remainder):
                    packages.append(raw);self.drop(start,end)
                    if re.search(r"\d+\s*\(\s*\d+|\d+\s*/\s*\d+",raw):
                        self.issue("packaging_alternatives","В упаковке указаны альтернативные количества; одно значение не выбиралось.")
        if packages:self.extra["packaging_raw"]=packages

    def base_name(self):
        characters=list(self.name)
        removed=set()
        for start,end in self.remove:
            for index in range(start,end):characters[index]=" ";removed.add(index)
        # Hyphens adjoining an extracted span are attribute delimiters. Preserve
        # model hyphens such as PR - 01 and ZCG-40KGS, which do not adjoin a span.
        for index,char in enumerate(self.name):
            if char not in "-–—":continue
            left=index-1;right=index+1
            while left>=0 and self.name[left].isspace():left-=1
            while right<len(self.name) and self.name[right].isspace():right+=1
            if left in removed or right in removed:characters[index]=" "
        base="".join(characters)
        # Empty quote pairs can remain after removal of an attribute in quotes.
        for pattern in (r'"([^"\n]*)"',r"'([^'\n]*)'",r'«([^»\n]*)»',r'“([^”\n]*)”',r'„([^“\n]*)“'):
            base=re.sub(pattern,lambda match:" " if not match.group(1).strip() else match.group(),base)
        for _ in range(4):
            base=re.sub(r"\(\s*[,/\\\-]*\s*\)"," ",base)
            base=re.sub(r"[,;]\s*(?=[,;)]|$)"," ",base)
        base=re.sub(r"(?<=\s)[,;]+|[,;]+(?=\s*$)"," ",base)
        base=re.sub(r"(?<=\s)[+*/&\\]+(?=\s|$)"," ",base)
        base=re.sub(r"\s*-\s*(?=[),;]|$)"," ",base)
        base=re.sub(r"(?<=\s)-(?=(?:\d+BB|B\d+)\b)","",base)
        base=re.sub(r"\s+"," ",base).strip(" \t-_,;/*&"+chr(92))
        base=re.sub(r"(?<=\s)[.,;]+(?=\s*[),]|$)","",base)
        base=re.sub(r"[,;]\s*(?=[.,;)]|$)","",base)
        base=re.sub(r"\s+"," ",base)
        base=re.sub(r"\s+([,)])",r"\1",base)
        base=re.sub(r"\(\s+","(",base)
        base=re.sub(r"(?:\s+-\s*){2,}"," ",base)
        base=re.sub(r"\(\s*[-–—]+\s*", "(",base)
        base=re.sub(r"\s+", " ",base).strip(" \t-–—_,;/*&"+chr(92))
        self.row["base_name"]=base or None
        if self.extra.get("unresolved_attributes"):
            self.extra["base_name_policy"]="Нерасшифрованные модельные/служебные коды сохранены; это буквальная основа, не идентификатор общей модели."

    def finish(self):
        if not isinstance(self.name,str) or not self.name.strip():
            self.issue("invalid_name","Название отсутствует или не является непустой строкой.")
        else:
            self.lexical();self.colors();self.measurements();self.packaging();self.base_name()
            if self.kind=="service":self.inform("service_record","Услуга: физические размер и цвет могут быть неприменимы.")
            if self.kind=="advertising":self.inform("advertising_record","Рекламная/выставочная позиция.")
            if self.cid in self.dictionary.config["known_category_mismatch_ids"]:
                self.inform("category_name_mismatch","Название исходной категории не соответствует виду изделия; category_id и путь сохранены.")
        if self.cid not in self.categories.by_id:self.issue("category_not_found","Категория товара отсутствует в файле категорий.")
        for code in self.categories.issues.get(self.cid,[]):self.issue(code,"Неполный или циклический путь категории.")
        if not self.row["base_name"] and isinstance(self.name,str) and self.name.strip():
            self.issue("base_name_empty_after_extraction","После извлечения атрибутов не осталось названия модели.")
        self.extra["review_reason_codes"]=sorted(self.reasons)
        if self.information:self.extra["information_codes"]=sorted(self.information)
        self.row["extra_attributes"]=self.extra
        self.row["needs_review"]=bool(self.reasons)
        def priority(code):
            if code.startswith("conflicting_") or code in ("brand_marking_conflict","ambiguous_side","multiple_unassigned_colors"):return (0,code)
            if code.startswith("invalid_") or code=="multiple_size_expressions":return (1,code)
            if code in ("unresolved_color","unresolved_color_combination","unresolved_color_alias_expression"):return (2,code)
            return (3,code)
        ordered=sorted(self.reasons,key=priority)
        self.extra["primary_review_reason_code"]=ordered[0] if ordered else None
        self.row["review_reason"]=self.reasons[ordered[0]][0] if ordered else None
        self.row["notes"]=" ".join([text for code in ordered for text in self.reasons[code]]+
            [text for code in sorted(self.information) for text in self.information[code]])
        return self.row


def write_csv(path,columns,rows,null_literal="null"):
    temporary=path.with_suffix(path.suffix+".tmp")
    with temporary.open("w",encoding="utf-8-sig",newline="") as stream:
        writer=csv.DictWriter(stream,fieldnames=columns,extrasaction="raise")
        writer.writeheader()
        for row in rows:
            serialized={}
            for key in columns:
                value=row[key]
                if value is None:serialized[key]=null_literal
                elif isinstance(value,bool):serialized[key]="true" if value else "false"
                elif isinstance(value,(dict,list)):serialized[key]=json.dumps(value,ensure_ascii=False,separators=(",",":"))
                else:serialized[key]=value
            writer.writerow(serialized)
    temporary.replace(path)


def summarize(rows,products,categories,input_paths,dictionary_path,dictionary):
    by_category=defaultdict(list)
    reasons=Counter()
    for row in rows:
        by_category[row["category_id"]].append(row)
        reasons.update(row["extra_attributes"]["review_reason_codes"])
    baseline=dictionary.data.get("revision_baseline",{})
    baseline_matches=baseline.get("input_hashes")==[source_hash(path) for path in input_paths]
    previous={item["category_id"]:item for item in baseline.get("categories",[])} if baseline_matches else {}
    category_rows=[]
    all_cids=set(categories.by_id)|set(by_category)
    for cid in sorted(all_cids,key=lambda key:(categories.paths.get(key,""),str(key))):
        records=by_category[cid]
        category_rows.append(dict(category_id=cid,category_path=categories.paths.get(cid),products_total=len(records),
            fully_recognized=sum(not row["needs_review"] for row in records),needs_review=sum(row["needs_review"] for row in records),
            color_recognized=sum(row["color_code"] is not None for row in records),size_found=sum(row["size"] is not None for row in records),
            needs_review_before=previous[cid]["needs_review"] if cid in previous else None,
            needs_review_change=sum(row["needs_review"] for row in records)-previous[cid]["needs_review"] if cid in previous else None))
    by_id={row["product_id"]:row for row in rows}
    examples=[dict(item,base_name_after=by_id[item["product_id"]]["base_name"]) for item in baseline.get("base_name_examples",[])
        if baseline_matches and item["product_id"] in by_id]
    return dict(parser_version=VERSION,products_total=len(rows),fully_recognized=sum(not row["needs_review"] for row in rows),
        needs_review=sum(row["needs_review"] for row in rows),color_recognized=sum(row["color_code"] is not None for row in rows),
        size_found=sum(row["size"] is not None for row in rows),base_name_found=sum(row["base_name"] is not None for row in rows),
        diameter_found=sum(row["size_diameter"] is not None for row in rows),secondary_color_recognized=sum(row["color_secondary_code"] is not None for row in rows),
        needs_review_before=baseline.get("needs_review") if baseline_matches else None,
        needs_review_change=sum(row["needs_review"] for row in rows)-baseline["needs_review"] if baseline_matches else None,
        baseline_comparison_applies=baseline_matches,base_name_examples=examples,
        assistant_selection_notice=dictionary.data.get("assistant_selection_notice"),
        assistant_selected_mappings=dictionary.data.get("assistant_selected_mappings",[]),dictionary_sources=dictionary.data["sources"],
        missing_attributes=dict(color=sum(row["color_raw"] is None for row in rows),size=sum(row["size"] is None and not row["extra_attributes"].get("measurements") for row in rows),side=sum(row["side"] is None for row in rows)),
        sources=[dict(path=str(path.resolve()),sha256=source_hash(path)) for path in input_paths],
        dictionary=dict(path=str(dictionary_path.resolve()),sha256=source_hash(dictionary_path)),
        included_source_flags=dict(is_deleted_true=sum(p.get("IsDeleted") is True for p in products),
            export_to_site_false=sum(p.get("ExportToSite") is False for p in products)),
        fully_recognized_definition="Без проверки (fully_recognized) означает needs_review=false: в найденных признаках нет обнаруженных неоднозначностей, противоречий и нерасшифрованных значений. Это не означает наличие всех атрибутов. Отсутствующие признаки — null без флага проверки.",
        null_representation="В CSV отсутствующее значение — буквальная строка null; внутри extra_attributes — JSON null.",
        numeric_policy="Линейные размеры без единиц считаются мм по указанию пользователя. См и м пересчитаны в мм. Оси заполняются по явной подписи, правилу плоской дверной петли либо D-диаметр × длина у круглых петель/колпачков. При неоднозначном порядке оси null, выражение сохранено в size/measurements и выставлен review.",
        context_guards="NP-02/SC-04 и другие префиксы перед -цифрой сохранены как модели. S в Пелікан S и PE70-S не назначен цветом. PC/РС учитывается как хром только у FZB. 40KGS внутри модели не является найденной массой.",
        reason_counts=dict(reasons.most_common()),primary_reason_counts=dict(Counter(row["extra_attributes"]["primary_review_reason_code"] for row in rows if row["needs_review"]).most_common()),categories=category_rows)


def render_report(path,summary,rows):
    esc=lambda value:"null" if value is None else html.escape(str(value),quote=True)
    def table(headers,records):
        return '<div class="table-wrap"><table><thead><tr>'+''.join('<th>'+esc(h)+'</th>' for h in headers)+'</tr></thead><tbody>'+''.join('<tr>'+''.join('<td>'+esc(v)+'</td>' for v in row)+'</tr>' for row in records)+'</tbody></table></div>'
    parts=['<!doctype html><html lang="ru"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>KedrStore — результат разметки</title><style>body{font:16px/1.5 system-ui,Segoe UI,sans-serif;color:#1b3042;background:#f4f6f8;margin:0}main{max-width:1420px;margin:auto;padding:28px}h1{font-size:30px}table{width:100%;border-collapse:collapse;background:white;margin:18px 0}th,td{border:1px solid #d7e0e8;padding:10px;text-align:left;vertical-align:top}th{background:#e9eff5}.table-wrap{overflow:auto}.notice{padding:18px;background:#fff2d6;border-left:4px solid #a27820}code{overflow-wrap:anywhere}a{color:#175c8a}li{margin:8px 0}</style><main>',
        '<h1>KedrStore: результат разметки товаров</h1>',
        '<p><a href="products_attributes.csv">products_attributes.csv</a> · <a href="category_report.csv">Отчёт по категориям</a> · <a href="dictionaries.json">Словари</a> · <a href="extract_attributes.py">Python-скрипт</a></p>',
        '<div class="notice">needs_review=true только для найденных, но неоднозначных, противоречивых или нерасшифрованных признаков. Отсутствие признака не вызывает проверку. В review_reason одна главная причина, подробности в notes. Исходные ID, названия и категории сохранены.</div>']
    parts.append(table(["Показатель","Количество"],[("Обработано",summary["products_total"]),("Без флага проверки",summary["fully_recognized"]),("needs_review до",summary["needs_review_before"]),("needs_review после",summary["needs_review"]),("Изменение needs_review",summary["needs_review_change"]),("Нормализован основной цвет",summary["color_recognized"]),("Нормализован второй цвет",summary["secondary_color_recognized"]),("Найдено основное размерное выражение",summary["size_found"]),("Извлечён диаметр",summary["diameter_found"])]))
    parts+=['<p>'+esc(summary["fully_recognized_definition"])+'</p>','<p>'+esc(summary["null_representation"])+'</p>','<p>'+esc(summary["numeric_policy"])+'</p>','<p>'+esc(summary["context_guards"])+'</p>',
        '<p>Размеры в скобках обрабатываются теми же правилами, что и вне скобок. Для «довжина 250 мм, діаметр 18 мм» заполнены size_length=250 и size_diameter=18. У «60*20*20мм» без подписей осей сохраняются все три компонента; последнее число не объявлено толщиной, поскольку у этих петель отдельно встречается толщина 1.2 мм или 1.5 мм.</p>',
        '<p>Масса, усилие, межосевое расстояние, части цилиндра, упаковка и размеры компонентов находятся в extra_attributes. Неуказанная сторона не превращена в универсальную, неизвестные подшипники — в ноль. Все '+str(summary["included_source_flags"]["is_deleted_true"] )+' удалённых записей и '+str(summary["included_source_flags"]["export_to_site_false"] )+' записей без ExportToSite включены.</p>',
        '<h2>По категориям</h2><p>Прямые количества по исходному CategoryId. Строки структурных категорий с нулём сохранены; дочерние товары в них повторно не учитываются.</p>']
    parts.append(table(["ID","Полный путь","Всего","Без проверки","Review до","Review после","Изменение","Цвет","Размер"],[[c["category_id"],c["category_path"],c["products_total"],c["fully_recognized"],c["needs_review_before"],c["needs_review"],c["needs_review_change"],c["color_recognized"],c["size_found"]] for c in summary["categories"]]))
    parts.append('<h2>Подбор расшифровок ассистентом</h2><p>'+esc(summary["assistant_selection_notice"] )+' Для каждой строки указан контекст применения; user_confirmed=false. Первый компонент сочетания записан в color_code/name, второй — в color_secondary_code/name. Это порядок записи, а не указание наружной и внутренней стороны.</p>')
    mapping_rows=[]
    for item in summary["assistant_selected_mappings"]:
        refs=[summary["dictionary_sources"][key] for key in item["source_refs"]]
        mapping_rows.append([item["raw"],item["normalized_code"],item["normalized_value_uk"],item["status"],item.get("product_count"),item.get("note",item.get("context_pattern","")),"; ".join(source["title"]+" — "+source["url"] for source in refs)])
    parts.append(table(["Вариант","Код","Украинское значение","Решение","Товаров с вариантом","Контекст/оговорка","Источники"],mapping_rows))
    for key,source in summary["dictionary_sources"].items():
        if key.startswith("assistant_") and source.get("url"):
            parts.append('<p><a href="'+esc(source["url"])+'">'+esc(source["title"])+'</a></p>')
    parts.append('<h2>20 примеров до/после: base_name</h2><p>Выбраны 20 строк, в которых base_name изменился после исправлений. Исходное название сохранено рядом для проверки.</p>')
    parts.append(table(["ID","Исходное название (без изменений)","base_name в предыдущей версии","base_name после исправления"],[[e["product_id"],e["original_name"],e["base_name_before"],e["base_name_after"]] for e in summary["base_name_examples"]]))
    variant_examples=[row for row in rows if row["product_id"] in (6140,6141,6142,6143,6144,6145,8036,8037,8056)]
    if variant_examples:
        parts.append('<h2>Одинаковая основа у цветовых вариантов одной модели</h2>')
        parts.append(table(["ID","Исходное название","base_name","Цвет","Сторона"],[[row["product_id"],row["original_name"],row["base_name"],row["color_name"],row["side"]] for row in variant_examples]))
    parts.append('<h2>Причины проверки</h2><p>Один товар может иметь несколько причин; количества не суммируются.</p>')
    parts.append(table(["Главная причина (код)","Товаров"],summary["primary_reason_counts"].items()))
    parts.append('<p>Все найденные причины, включая дополнительные:</p>')
    parts.append(table(["Код причины","Товаров"],summary["reason_counts"].items()))
    parts.append('<h2>Примеры строк</h2>')
    selected=[]
    wanted={3292,6140,65,5963,8350,25902,2207,8063,143,26567,6381}
    selected.extend(row for row in rows if row["product_id"] in wanted)
    if not selected:selected=rows[:10]
    parts.append(table(["ID","Исходное название","Базовое название","Размер","Цвет","Второй цвет","Сторона","needs_review","Главная причина"],[[r["product_id"],r["original_name"],r["base_name"],r["size"],r["color_name"],r["color_secondary_name"],r["side"],r["needs_review"],r["review_reason"]] for r in selected]))
    parts.append('<h2>Повторный запуск</h2><pre>python extract_attributes.py --products Products.json --categories ProductCategories.json --dictionaries dictionaries.json --outdir result</pre><p>Python 3.10+, внешние библиотеки не нужны. Скрипт читает файлы, применяет словари и регулярные выражения, создаёт CSV и отчёты. Он не обращается к PostgreSQL или сети.</p><h2>Контроль источников</h2>')
    parts.append(table(["Файл","SHA-256"],[[s["path"],s["sha256"]] for s in summary["sources"]]+[[summary["dictionary"]["path"],summary["dictionary"]["sha256"]]]))
    parts.append('</main></html>')
    path.write_text('\n'.join(parts),encoding="utf-8")


def main():
    here=Path(__file__).resolve().parent
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--products",type=Path,default=here/"_Products__202611050133.json")
    parser.add_argument("--categories",type=Path,default=here/"_ProductCategories__202611050134.json")
    parser.add_argument("--dictionaries",type=Path,default=here/"dictionaries.json")
    parser.add_argument("--outdir",type=Path,default=here)
    args=parser.parse_args()
    products=load_rows(args.products,"Products")
    categories=Categories(load_rows(args.categories,"ProductCategories"))
    if any(p.get("Id") is None for p in products):raise ValueError("A product has no Id; refusing to invent an identifier")
    identifiers=[str(p["Id"]) for p in products]
    if len(set(identifiers))!=len(identifiers):raise ValueError("Duplicate product IDs or colliding CSV representations")
    dictionary=Dictionary(args.dictionaries,categories)
    rows=[Extraction(p,dictionary,categories).finish() for p in products]
    assert len(rows)==len(products)
    assert all(row["product_id"]==p["Id"] and row["category_id"]==p.get("CategoryId") and row["original_name"]==p.get("Name") for row,p in zip(rows,products))
    assert all(row["notes"] for row in rows if row["needs_review"])
    args.outdir.mkdir(parents=True,exist_ok=True)
    if (args.outdir/"dictionaries.json").resolve()!=args.dictionaries.resolve():
        (args.outdir/"dictionaries.json").write_bytes(args.dictionaries.read_bytes())
    if (args.outdir/"extract_attributes.py").resolve()!=Path(__file__).resolve():
        (args.outdir/"extract_attributes.py").write_bytes(Path(__file__).read_bytes())
    write_csv(args.outdir/"products_attributes.csv",COLUMNS,rows,dictionary.config["null_csv_literal"])
    summary=summarize(rows,products,categories,[args.products,args.categories],args.dictionaries,dictionary)
    category_columns=["category_id","category_path","products_total","fully_recognized","needs_review","color_recognized","size_found","needs_review_before","needs_review_change"]
    write_csv(args.outdir/"category_report.csv",category_columns,summary["categories"])
    (args.outdir/"report.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding="utf-8")
    render_report(args.outdir/"report.html",summary,rows)
    if summary["base_name_examples"]:
        write_csv(args.outdir/"base_name_examples.csv",["product_id","original_name","base_name_before","base_name_after"],summary["base_name_examples"])
    print(json.dumps({key:summary[key] for key in ("products_total","fully_recognized","needs_review","color_recognized","size_found","base_name_found")},ensure_ascii=False,indent=2))


if __name__=="__main__":main()
