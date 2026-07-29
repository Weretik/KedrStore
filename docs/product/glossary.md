# KedrStore product glossary

This document contains only stable business terms that recur across catalog and feature specifications. Feature-specific rules, mappings, and acceptance criteria remain in the feature's `requirements/`.

## Product

A catalog item sold or displayed by KedrStore. A product has a name, category, 1C identifier, stock, prices, quantity per pack, and publication status.

## Catalog

The structured collection of products, categories, prices, stock, and product-list data available to the site and administrators.

## Product category

A node in the catalog hierarchy used to group products. A category has a 1C identifier, name, slug, path, and an optional parent category.

## 1C catalog root

A top-level source grouping passed to 1C catalog import operations. KedrStore currently imports the Doors, Hardware, and Cosmos roots.

## Doors

The 1C catalog root and product family for door products, including entrance and interior doors.

## Entrance door

A door intended for an external or building-entry opening.

## Interior door

A door intended for an opening between rooms within a building.

## Hardware

The 1C catalog root and product family for fittings and accessories used with doors or other building elements.

## Door hardware

Fittings and accessories that enable, secure, control, or complete a door installation. Typical hardware categories include hinges, locks, handles, cylinders, door viewers, and related accessories.

## Hinge

A hardware component that connects a door to its frame and allows the door to pivot.

## Lock

A hardware component that secures a door or access point. The catalog may distinguish, for example, mortise, rim, lever, cylinder, padlock, and handle-set locks.

## Handle

A hardware component used to operate a door. Catalog variants may include lever handles on a rose or plate, knobs, and stainless-steel handles.

## Cylinder

A replaceable locking mechanism used with compatible cylinder locks.

## Door viewer

A viewing device installed in a door to allow observation through it while it remains closed.

## Cosmos

A named 1C catalog root (`Космос`) imported into KedrStore. It is a source-root designation, not a product category inferred from the product name. Products imported from this root are assigned to its configured local fallback category.

## 1C identifier

The identifier supplied by the 1C source system for a product, category, price type, or catalog root. It is used to reconcile imported data with local catalog records.

## Product slug

A stable URL-friendly identifier for a product in the public catalog.

## Category path

The ordered hierarchical path of a product category. It identifies the category's position in the catalog tree.

## Price type

A named price classification received from 1C and used to distinguish prices for the same product.

## Product price

The monetary amount of a product for a specific price type and currency.

## Stock

The available quantity reported for a product by 1C and stored in the catalog. A stock update changes availability data; it does not create or remove the product.

## Quantity per pack

The number of product units included in one pack.

## Product scheme

An optional scheme or diagram associated with a product and provided by the catalog source.

## Export to site

The publication flag that controls whether a product is included in the public product list. Products not exported to the site may still be imported, stocked, priced, and visible to administrators.

## Product list projection

The read-optimised catalog representation used by product-list queries. It is rebuilt from local catalog data after relevant imports.
