# PontelloImport - Product Catalog Management System

## Documentation - Current State & Implementation Guide

**Last Updated:** February 2026
**Version:** 1.0
**Framework:** ASP.NET Core 9.0 MVC
**Database:** SQLite with Entity Framework Core

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture & Technology Stack](#architecture--technology-stack)
3. [Database Schema](#database-schema)
4. [Core Features](#core-features)
5. [Business Logic & Workflows](#business-logic--workflows)
6. [UI Implementation](#ui-implementation)
7. [API Endpoints](#api-endpoints)
8. [Data Seeding](#data-seeding)
9. [Validation Rules](#validation-rules)
10. [Special Patterns](#special-patterns)
11. [Known Issues](#known-issues)
12. [Future Enhancements](#future-enhancements)

---

## Project Overview

PontelloImport is a specialized e-commerce product catalog management system designed for **Pontello Motorsports**, a go-kart racing equipment supplier. The application enables administrators to manage products, variants, and attributes through a sophisticated multi-step workflow with comprehensive filtering and search capabilities.

### Key Capabilities

- ✅ Hierarchical product management (Parent products → Multiple SKU variants)
- ✅ Flexible attribute system (Specifications vs. Customer-selectable options)
- ✅ Review-before-save workflow (Form → Review → Confirm)
- ✅ Advanced filtering & search (5 filter dimensions + pagination)
- ✅ Status lifecycle management (Draft → Published → Archived)
- ✅ Complete audit trail (Automatic user/timestamp tracking)
- ✅ Real-time validation (Client-side + Server-side)
- ✅ Soft delete architecture (Data preservation + Restore capability)

---

## Architecture & Technology Stack

### Framework & Dependencies

```xml
<TargetFramework>net9.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

**Core Packages:**
- `Microsoft.EntityFrameworkCore.Sqlite` - Data access layer
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - Authentication
- `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` - Database diagnostics

### Project Structure

```
PontelloImport/
├── Controllers/
│   ├── HomeController.cs
│   ├── ProductsController.cs
│   └── ProductVariantsController.cs (Main CRUD controller - 900 lines)
├── Data/
│   ├── ApplicationDbContext.cs (Identity)
│   ├── PontelloDbContext.cs (Business domain)
│   ├── PontelloDbInitializer.cs (Seeding logic)
│   └── Migrations/
│       ├── PIMigrations/ (Business schema)
│       └── (Identity migrations)
├── Models/
│   ├── Product.cs
│   ├── ProductVariant.cs
│   ├── ProductAttribute.cs
│   ├── Vendor.cs
│   ├── ProductCategory.cs
│   ├── CreateProductViewModel.cs
│   ├── PaginatedList.cs
│   └── Auditable.cs (Base class)
├── Views/
│   ├── ProductVariants/
│   │   ├── Index.cshtml (List with filters)
│   │   ├── Create.cshtml (Multi-card form)
│   │   ├── Review.cshtml (Confirmation page)
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Delete.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _AdminLayout.cshtml
└── wwwroot/ (Static assets)
```

### Dual DbContext Architecture

**PontelloDbContext** - Business Domain
- Products, ProductVariants, ProductAttributes
- Vendors, ProductCategories
- Automatic audit trail via SaveChanges override

**ApplicationDbContext** - Identity
- AspNetUsers, AspNetRoles, AspNetUserTokens, etc.
- Standard ASP.NET Core Identity tables

**Configuration:**
- Both contexts share same SQLite database: `PontelloImportDatabase.db`
- Session-based TempData provider with 30-minute idle timeout
- Supports large data transfer for review-before-save flows

---

## Database Schema

### Entity Relationship Diagram

```
┌─────────────┐          ┌─────────────────┐          ┌──────────────────┐
│   Vendor    │ 1───────∞│    Product      │ ∞───────1│ ProductCategory  │
└─────────────┘          └─────────────────┘          └──────────────────┘
                                  │
                                  │ 1
                                  │
                                  │
                                  ∞
                         ┌─────────────────┐
                         │ ProductVariant  │
                         └─────────────────┘
                                  │
                                  │ 1
                                  │
                                  │
                                  ∞
                      ┌────────────────────────┐
                      │  ProductAttribute      │
                      └────────────────────────┘
```

### Core Entities

#### 1. Product (Parent Container)

**Purpose:** Logical grouping for product variants

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| ProductID | int | PK, Identity | Auto-generated ID |
| Title | string(255) | Required | Product display name |
| Handle | string(255) | Unique, Indexed | URL-friendly slug (auto-generated) |
| Description | text | Optional | Marketing description |
| Type | string(100) | Optional | Product category type |
| Tags | string | Optional | Comma-separated searchable tags |
| Status | enum | Required | Draft / Published / Archived |
| IsActive | bool | Required | Soft delete flag |
| VendorID | int? | FK → Vendor | Supplier reference |
| ProductCategoryID | int? | FK → ProductCategory | Category reference |
| CreatedBy | string | Auto | User ID from claims |
| CreatedDate | DateTime | Auto | UTC timestamp |
| ModifiedBy | string | Auto | User ID from claims |
| ModifiedDate | DateTime | Auto | UTC timestamp |

**Indexes:**
- Handle (UNIQUE)
- Status + IsActive (Composite)

---

#### 2. ProductVariant (SKU-Specific Variants)

**Purpose:** Concrete sellable items with unique SKU, pricing, and inventory

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| VariantID | int | PK, Identity | Auto-generated ID |
| ProductID | int? | FK → Product (nullable) | Parent product reference |
| Title | string(255) | Required | Variant-specific name |
| Handle | string(255) | Unique, Indexed | URL-friendly slug |
| SKU | string(50) | Required, Unique | Stock Keeping Unit |
| **Pricing** |
| Price | decimal(18,2) | Required, 0.01-999999.99 | Selling price |
| CompareAtPrice | decimal(18,2) | Optional, 0.01-999999.99 | Original price (for discounts) |
| **Inventory** |
| InventoryQuantity | int | Required, ≥0 | Stock count |
| InventoryPolicy | string(50) | Default: "deny" | Out-of-stock behavior |
| Weight | int? | Optional | Weight in grams |
| **Shipping** |
| Barcode | string(100) | Optional | UPC/EAN code |
| RequiresShipping | bool | Default: true | Physical goods flag |
| IsTaxable | bool | Default: true | Tax applicability |
| **Metadata** |
| Description | text | Optional | Variant-specific details |
| Type | string(100) | Optional | Variant classification |
| Tags | string | Optional | Search tags |
| Status | enum | Required | Draft / Published / Archived |
| IsActive | bool | Required | Soft delete flag |
| CreatedBy/Date | Audit | Auto | Audit trail |
| ModifiedBy/Date | Audit | Auto | Audit trail |

**SKU Format Validation:**
- Regex: `^[A-Z]{2,4}-\d{3,7}(-[A-Z]{1,3})?$`
- Examples: `BA-4501`, `VX-2200`, `MG-3100-SM`

**Indexes:**
- SKU (UNIQUE)
- Handle (UNIQUE)
- ProductID + IsActive (Composite)
- Status + IsActive (Composite)
- Price, Weight (Filtering/Sorting)

---

#### 3. ProductAttribute (Flexible Metadata)

**Purpose:** Custom name-value pairs for specifications or variant options

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| AttributeID | int | PK, Identity | Auto-generated ID |
| VariantID | int | FK → ProductVariant, Cascade Delete | Parent variant |
| AttributeName | string(100) | Required | Attribute key (e.g., "Color") |
| AttributeValue | string(500) | Required | Attribute value (e.g., "Powder Coated Black") |
| IsVariantAttribute | bool | Default: false | True = Customer-selectable, False = Specification |
| DisplayOrder | int | Default: 0 | UI ordering |
| CreatedBy/Date | Audit | Auto | Audit trail |
| ModifiedBy/Date | Audit | Auto | Audit trail |

**Indexes:**
- AttributeName + AttributeValue (Composite, for searches)

**Predefined Attribute Names (UI Dropdown):**
- Size, Color, Material, Finish, Diameter, Length, Width, Height
- Compound, Condition, Thread Type, Compatibility, Other (custom input)

---

#### 4. Vendor (Supplier Management)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| VendorID | int | PK, Identity | Auto-generated ID |
| VendorName | string(255) | Required, Unique | Display name |
| VendorSlug | string(255) | Unique | URL-friendly identifier |
| ContactName | string(255) | Optional | Primary contact |
| ContactEmail | string(255) | Optional | Email address |
| ContactPhone | string(50) | Optional | Phone number |
| Website | string(500) | Optional | Vendor URL |
| Country | string(100) | Optional | Location |
| Notes | text | Optional | Internal notes |
| IsActive | bool | Default: true | Soft delete flag |
| CreatedBy/Date | Audit | Auto | Audit trail |

**Seeded Vendors:** Birel ART, Vortex Engines, MG Tires, CRG, Tony Kart, OTK, Pontello Motorsports, Phantom Racing, Authentic Phantom

---

#### 5. ProductCategory (Hierarchical Categories)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| CategoryID | int | PK, Identity | Auto-generated ID |
| CategoryName | string(255) | Required, Unique | Display name |
| CategorySlug | string(255) | Unique | URL-friendly identifier |
| ParentCategoryID | int? | FK → ProductCategory (self-reference) | Parent category for nesting |
| DisplayOrder | int | Default: 0 | UI ordering |
| CategoryDescription | text | Optional | Marketing description |
| IsActive | bool | Default: true | Soft delete flag |
| CreatedBy/Date | Audit | Auto | Audit trail |

**Supports:** Unlimited nesting depth (e.g., Chassis → Front Chassis → Left Nerf Bar)

**Seeded Categories:** Chassis, Tires, Engines, Accessories, Safety Equipment, Tools

---

### Audit Trail System

**Implementation:**
- All entities inherit from `Auditable` base class
- `PontelloDbContext` overrides `SaveChanges()` to auto-populate audit fields
- User ID extracted from `HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)`

**Fields:**
- `CreatedBy` (string) - User ID on insert
- `CreatedDate` (DateTime) - UTC timestamp on insert
- `ModifiedBy` (string) - User ID on update
- `ModifiedDate` (DateTime) - UTC timestamp on every update

---

## Core Features

### 1. Product Variant Management (Main Module)

#### List View (`/ProductVariants/Index`)

**Features:**
- **Pagination:** 10 items/page (configurable 1-100)
- **Search:** Real-time search by Title or SKU (case-insensitive)
- **Filters:**
  - Category (dropdown, active categories only)
  - Vendor (dropdown, active vendors only)
  - Product Type (dropdown, distinct types from database)
  - Stock Status (In Stock >5 / Low Stock 1-5 / Out of Stock 0)
- **Status Tabs:** All (non-archived) / Published / Draft / Archived
  - Dynamic counts displayed on each tab
  - Counts respect active search/filter state
- **Sorting:** Order by CreatedDate descending (newest first)
- **Clear Filters:** Conditional button appears when filters are active

**UI Components:**
- Responsive Bootstrap grid
- Color-coded status badges (Published=green, Draft=yellow, Archived=gray)
- Inventory indicators (InStock=green, LowStock=orange, OutOfStock=red)
- Action buttons: View Details, Edit, Delete/Archive
- Sticky filter bar

---

#### Create Flow (`/ProductVariants/Create`)

**Workflow:** Form → Review → Save

**Step 1: Create Form (GET)**

Multi-card layout:

1. **Basic Information Card:**
   - Variant Title (required)
   - Variant SKU (required, uppercase auto-convert)
   - Vendor (dropdown, required)
   - Product Category (dropdown, required)
   - Product Type (hybrid dropdown with "Other - Please Specify" option)
   - Description (rich textarea)
   - Tags (comma-separated)

2. **Pricing & Inventory Card:**
   - Price (required, currency input with $ prefix)
   - Compare At Price (optional, for discounts)
   - Inventory Quantity (required, integer ≥0)
   - Weight (input + unit selector: g/kg/lb/oz, auto-converted to grams)
   - Barcode (optional)

3. **Shipping & Tax Card:**
   - Requires Shipping (checkbox, default: true)
   - Is Taxable (checkbox, default: true)

4. **Attributes Card:**
   - Dynamic multi-row input (add/remove rows)
   - Attribute Name (dropdown: Size/Color/Material/... or custom)
   - Attribute Value (text input)
   - IsVariantAttribute (checkbox: customer-facing option vs. specification)
   - Display Order (integer, defaults to row index)

**Client-Side Features:**
- SKU auto-uppercase on input
- Real-time SKU uniqueness check (AJAX, debounced 500ms)
- Real-time Title uniqueness check (AJAX, debounced 500ms)
- Weight unit conversion to grams before submission
- Validation summary alert (Bootstrap dismissible)
- Demo data auto-fill shortcut: `Ctrl + ` ` (backtick) - **⚠️ See Known Issues**

**Step 2: POST to Create Action**

1. Validate ModelState (server-side)
2. Auto-copy Variant.Title → Product.Title (for simple products)
3. Generate URL-friendly handles from titles
4. Serialize `CreateProductViewModel` to JSON
5. Store in `TempData["CreateProductReviewData"]`
6. Redirect to Review page

**Step 3: Review Page (GET)**

- Read-only display of all entered data
- Vendor/Category names looked up from IDs
- Tags rendered as Bootstrap badges
- Inventory status color indicators
- Discount percentage calculated (if CompareAtPrice set)
- Attributes listed with variant/spec distinction

**Actions:**
- **Back to Edit:** Returns to Create form with data pre-filled (uses `fromReview=true` parameter)
- **Save as Draft:** Sets Status=Draft, IsActive=false
- **Publish:** Sets Status=Published, IsActive=true

**Step 4: ConfirmCreate Action (POST)**

1. Deserialize JSON from TempData
2. Create **Product** entity first (get ProductID)
3. Create **ProductVariant** entity (link to ProductID)
4. Create **ProductAttribute** entities (link to VariantID)
5. Save all to database in single transaction
6. Clear TempData
7. Redirect to Details page with success message

**Error Handling:**
- Catch `DbUpdateException` for constraint violations
- Parse SQLite error messages (UNIQUE constraint failed)
- Show friendly error messages:
  - "The SKU 'BA-4501' is already in use."
  - "A product with this title already exists."
- Re-store TempData for user to fix and retry

---

#### Edit Flow (`/ProductVariants/Edit/{id}`)

**GET:**
1. Load existing ProductVariant with related Product and Attributes
2. Populate `CreateProductViewModel`
3. Display same form as Create (reused view)

**POST:**
1. Validate ModelState
2. Regenerate handles from titles
3. Update Product entity (Title, Handle, Vendor, Category, Description, Type, Tags, Status)
4. Update ProductVariant entity (all fields)
5. **Replace attributes:** Delete all existing, add new ones from form
6. Save changes
7. Redirect to Details with success message

**Constraint Handling:**
- Duplicate SKU → ModelState.AddModelError("Variant.SKU", "...")
- Duplicate Handle → ModelState.AddModelError("Variant.Title", "...")
- Re-populate dropdowns on error

---

#### Delete Flow (Soft Delete)

**GET: /ProductVariants/Delete/{id}**
- Display confirmation page with full product details

**POST: /ProductVariants/Delete/{id}**
1. Set `Status = ProductStatus.Archived`
2. Set `IsActive = false`
3. Save changes
4. Redirect to Details (not Index, to allow immediate restore)

**Note:** ProductAttributes are NOT deleted (cascade delete disabled for soft delete architecture)

---

#### Restore Flow

**POST: /ProductVariants/Restore/{id}**
1. Set `Status = ProductStatus.Draft`
2. Set `IsActive = true`
3. Save changes
4. Redirect to Details with success message

---

#### Publish/Unpublish

**POST: /ProductVariants/Publish/{id}**
- Status = Published, IsActive = true
- Success: "Product '{Title}' is now published and visible to dealers!"

**POST: /ProductVariants/Unpublish/{id}**
- Status = Draft, IsActive = false
- Success: "Product '{Title}' is now unpublished and hidden from dealers."

---

### 2. Product Management (Parent Products)

#### `/Products/Index`
- List all parent products with Vendor/Category eager loading
- Shows count of variants per product
- Action buttons: View, Edit, Delete (if no variants exist)

#### `/Products/Create`
- Simple form: Title, Vendor, Category, Description, Type, Tags
- Generates unique handle automatically

#### `/Products/Delete`
- **Business Rule:** Cannot delete if variants exist
- Error message: "Cannot delete product because it has X variant(s)."

---

## Business Logic & Workflows

### Handle Generation Algorithm

**Purpose:** Convert human-readable titles to URL-friendly slugs

**Steps:**
1. Convert to lowercase: `"Birel ART CRY-S12"` → `"birel art cry-s12"`
2. Replace special characters with spaces:
   - `/`, `°`, `"`, `'`, `(`, `)`, `[`, `]`, `&`, `½`, `¼`, `¾` → ` `
3. Replace all spaces with hyphens: `"birel art cry-s12"` → `"birel-art-cry-s12"`
4. Remove non-alphanumeric except hyphens: `[^a-z0-9-]` → removed
5. Collapse multiple hyphens: `"birel---art"` → `"birel-art"`
6. Trim leading/trailing hyphens: `"-birel-"` → `"birel"`

**Uniqueness Handling:**
- Initial: `"my-product"`
- If exists: Try `"my-product-1"`, `"my-product-2"`, etc.
- Implemented in `EnsureUniqueHandle()` (ProductsController only)

**Note:** ProductVariantsController does NOT auto-suffix; relies on user changing title to resolve conflicts.

---

### Status Lifecycle

```
         ┌─────────┐
         │  Draft  │◄──────────────┐
         └─────────┘               │
              │                    │
              │ Publish            │ Unpublish
              ▼                    │
      ┌──────────────┐             │
      │  Published   │─────────────┘
      └──────────────┘
              │
              │ Delete (Soft)
              ▼
        ┌──────────┐
        │ Archived │────┐
        └──────────┘    │
              ▲         │
              │         │ Restore
              └─────────┘
```

**Status Meanings:**
- **Draft:** Work in progress, not visible to customers (IsActive=false)
- **Published:** Active, visible to dealers/customers (IsActive=true)
- **Archived:** Soft deleted, hidden from default views (IsActive=false)

---

### Inventory Status Classification

**Business Rules:**
- **In Stock:** InventoryQuantity > 5 (Green indicator)
- **Low Stock:** 1 ≤ InventoryQuantity ≤ 5 (Orange/Yellow indicator)
- **Out of Stock:** InventoryQuantity = 0 (Red indicator)

**Filtering:**
- Applied in Index view via `stockStatus` parameter
- Affects count queries and displayed results

---

### Discount Calculation

**Formula:**
```csharp
if (CompareAtPrice.HasValue && CompareAtPrice > Price)
{
    decimal discount = CompareAtPrice.Value - Price;
    decimal percentage = (discount / CompareAtPrice.Value) * 100;
    // Display: "Save 12% ($50.00 off)"
}
```

---

## UI Implementation

### Layouts

**1. _AdminLayout.cshtml**
- Sidebar navigation with active link highlighting
- Top navbar with user menu
- Breadcrumbs
- Flash message area (TempData success/error alerts)

**2. _Layout.cshtml**
- Public-facing Bootstrap layout
- Header, footer, main content area
- Validation scripts partial

---

### Forms & Validation

**Client-Side:**
- jQuery Validation + Unobtrusive Validation
- Bootstrap validation states (is-invalid class)
- Real-time feedback for SKU/Title uniqueness (AJAX)
- Custom validation messages in `.field-instant-feedback` divs

**Server-Side:**
- Data Annotations on models ([Required], [Range], [RegularExpression])
- Custom ModelState error replacement for binding errors
- TempData for cross-request error messages

**Validation Summary:**
- Bootstrap alert-danger dismissible
- Shows all ModelState errors
- Positioned at top of form

---

### Dynamic Attributes Input

**Implementation:**
- JavaScript counter: `attributeCounter` tracks row count
- Template row cloned on "Add Attribute" button click
- Remove button deletes row (minimum 1 row enforced)
- Predefined attribute names in dropdown (Size, Color, Material, etc.)
- "Other - Please Specify" triggers custom input field

**HTML Structure:**
```html
<div id="attributesContainer">
    <div class="attribute-row">
        <select name="Attributes[0].AttributeName">
            <option value="Size">Size</option>
            <option value="Color">Color</option>
            <!-- ... -->
            <option value="Other">Other - Please Specify</option>
        </select>
        <input type="text" name="Attributes[0].AttributeValue" />
        <input type="checkbox" name="Attributes[0].IsVariantAttribute" />
        <input type="hidden" name="Attributes[0].DisplayOrder" value="0" />
        <button type="button" class="remove-attribute-btn">×</button>
    </div>
</div>
```

**Model Binding:**
- ASP.NET Core automatically binds `List<AttributeInputModel>` from indexed inputs

---

### Product Type Hybrid Dropdown

**Predefined Types:**
```csharp
"Chassis Components", "Engine Parts", "Safety Equipment",
"Accessories", "Tools", "Hardware", "Cleaning Supplies",
"Performance Parts", "Measurement Tools", "Hand Tools",
"Other - Please Specify"
```

**Behavior:**
- Select predefined type → Hidden `customTypeInput` field
- Select "Other" → Show text input for custom type
- On Edit: If current type not in list → Auto-select "Other" and populate custom input

**Implementation:**
```javascript
document.getElementById('productTypeDropdown').addEventListener('change', function() {
    const customContainer = document.getElementById('customTypeContainer');
    if (this.value === 'Other - Please Specify') {
        customContainer.style.display = 'block';
    } else {
        customContainer.style.display = 'none';
        document.getElementById('Product_Type').value = this.value;
    }
});
```

---

### Weight Unit Conversion

**Supported Units:**
- Grams (g) - Base unit stored in database
- Kilograms (kg)
- Pounds (lb)
- Ounces (oz)

**Conversion Logic (JavaScript):**
```javascript
function convertToGrams(value, unit) {
    switch(unit) {
        case 'kg': return value * 1000;
        case 'lb': return value * 453.592;
        case 'oz': return value * 28.3495;
        case 'g':
        default:   return value;
    }
}
```

**Form Submission:**
- Read `weightInput` (display value) and `weightUnit` (selected unit)
- Convert to grams
- Set `Variant.Weight` hidden field
- Submit to server

---

## API Endpoints

### AJAX Validation Endpoints

#### Check SKU Uniqueness
```
GET /ProductVariants/CheckSku?sku={value}&variantId={id}
```

**Response:**
```json
{ "isAvailable": true }  // or false
```

**Logic:**
- Returns `false` if another variant (excluding current variantId) uses this SKU
- Case-sensitive match (SKU is always uppercase)

---

#### Check Title Uniqueness
```
GET /ProductVariants/CheckTitle?title={value}&variantId={id}
```

**Response:**
```json
{ "isAvailable": true }  // or false
```

**Logic:**
- Generates handle from title
- Returns `false` if another variant (excluding current variantId) uses this handle

---

### RESTful Routes

| Method | Route | Action | Description |
|--------|-------|--------|-------------|
| GET | /ProductVariants | Index | List variants with filters |
| GET | /ProductVariants/Create | Create (GET) | Show create form |
| POST | /ProductVariants/Create | Create (POST) | Validate and redirect to Review |
| GET | /ProductVariants/Review | Review | Show confirmation page |
| POST | /ProductVariants/ConfirmCreate | ConfirmCreate | Save to database |
| GET | /ProductVariants/Edit/{id} | Edit (GET) | Show edit form |
| POST | /ProductVariants/Edit/{id} | Edit (POST) | Update variant |
| GET | /ProductVariants/Details/{id} | Details | View variant details |
| GET | /ProductVariants/Delete/{id} | Delete (GET) | Show delete confirmation |
| POST | /ProductVariants/Delete/{id} | DeleteConfirmed | Soft delete variant |
| POST | /ProductVariants/Restore/{id} | Restore | Restore archived variant |
| POST | /ProductVariants/Publish/{id} | Publish | Activate variant |
| POST | /ProductVariants/Unpublish/{id} | Unpublish | Deactivate variant |

---

## Data Seeding

### Initial Seed Data (PontelloDbInitializer.Seed)

**9 Vendors:**
1. Birel ART (Italy) - Premium chassis manufacturer
2. Vortex Engines (Italy) - Engine supplier
3. MG Tires (Italy) - Racing tire manufacturer
4. CRG (Italy) - Chassis brand
5. Tony Kart (Italy) - Chassis brand
6. OTK (Italy) - Parent company of multiple brands
7. Pontello Motorsports (USA) - House brand
8. Phantom Racing (USA) - Aftermarket parts
9. Authentic Phantom (USA) - OEM parts

**6 Product Categories:**
1. Chassis
2. Tires
3. Engines
4. Accessories
5. Safety Equipment
6. Tools

**8 Parent Products:**
1. Nerf Bar (Pontello Motorsports)
2. Racing Steering Wheel (Phantom Racing)
3. Tie Rod Assembly (CRG)
4. Floor Pan Kit (Birel ART)
5. Kart Cleaning Spray (Pontello Motorsports)
6. Racing Gloves (Safety Equipment)
7. Torque Wrench (Tools)
8. Tire Pressure Gauge (Tools)

**20+ Product Variants (Examples):**

**Nerf Bar - 6 Variants:**
- `PN-1001-L` - Powder Coated Black - Left ($89.99)
- `PN-1001-R` - Powder Coated Black - Right ($89.99)
- `PN-1002-L` - Stainless Steel - Left ($119.99)
- `PN-1002-R` - Stainless Steel - Right ($119.99)
- `PN-1003-L` - Chrome Plated - Left ($149.99)
- `PN-1003-R` - Chrome Plated - Right ($149.99)

**Racing Steering Wheel - 4 Variants:**
- `PH-2001-BK-10` - Black 10" ($79.95)
- `PH-2001-BK-12` - Black 12" ($84.95)
- `PH-2001-RD-10` - Red 10" ($79.95)
- `PH-2001-RD-12` - Red 12" ($84.95)

**Tie Rod Assembly - 3 Variants:**
- `CRG-3001-6` - 6" ($45.00)
- `CRG-3001-6.5` - 6.5" ($45.00)
- `CRG-3001-7` - 7" ($45.00)

**30+ Product Attributes:**
- Material (Aluminum 6061-T6, Chromoly Steel, etc.)
- Side (Left, Right)
- Finish (Powder Coated Black, Stainless Steel, Chrome)
- Diameter (30mm, 32mm, etc.)
- Grip Material (Suede, Rubber, Leather)
- Thread Type (M8 x 1.25, etc.)
- Certification (CIK-FIA Homologated, etc.)

**Seeding Strategy:**
- Conditional: Only insert if table is empty
- Runs on every app startup via `Program.cs`
- Uses `SaveChanges()` after each entity group

---

## Validation Rules

### Product Model

| Field | Rules |
|-------|-------|
| Title | Required, MaxLength(255) |
| Handle | MaxLength(255), Unique |
| Description | Optional |
| Type | MaxLength(100) |
| Tags | Optional |
| Status | Required (enum: Draft/Published/Archived) |
| VendorID | Required (FK) |
| ProductCategoryID | Required (FK) |

---

### ProductVariant Model

| Field | Rules |
|-------|-------|
| Title | Required, MaxLength(255) |
| SKU | Required, MaxLength(50), Unique, Regex: `^[A-Z]{2,4}-\d{3,7}(-[A-Z]{1,3})?$` |
| Price | Required, Range(0.01, 999999.99), DataType.Currency |
| CompareAtPrice | Range(0.01, 999999.99), DataType.Currency |
| InventoryQuantity | Required, Range(0, int.MaxValue) |
| InventoryPolicy | MaxLength(50) |
| Weight | Range(0, int.MaxValue) (grams) |
| Barcode | MaxLength(100) |
| RequiresShipping | Required (bool) |
| IsTaxable | Required (bool) |
| Description | Optional |
| Type | MaxLength(100) |
| Tags | Optional |
| Status | Required (enum) |
| Handle | MaxLength(255), Unique |

---

### ProductAttribute Model

| Field | Rules |
|-------|-------|
| AttributeName | Required, MaxLength(100) |
| AttributeValue | Required, MaxLength(500) |
| IsVariantAttribute | Required (bool) |
| DisplayOrder | Required (int) |

---

### Custom Validation Logic

**1. Binding Error Replacement (`ReplaceBindingErrorsWithRequiredMessages`)**

**Problem:** When a number field receives empty string, model binding fails BEFORE [Required] validation runs, producing unhelpful message: "The value '' is invalid."

**Solution:**
```csharp
var fieldMessages = new Dictionary<string, string>
{
    { "Variant.Price", "Price is required for product creation." },
    { "Variant.InventoryQuantity", "Inventory Quantity is required for product creation." },
    { "Variant.CompareAtPrice", "Compare At Price must be a positive value." },
    { "Variant.Weight", "Weight must be a valid number." }
};

foreach (var kvp in fieldMessages)
{
    if (ModelState.TryGetValue(kvp.Key, out var entry) && entry.Errors.Any())
    {
        var hasBindingError = entry.Errors.Any(e =>
            e.ErrorMessage.Contains("The value") && e.ErrorMessage.Contains("is not valid"));

        if (hasBindingError)
        {
            ModelState.Remove(kvp.Key);
            ModelState.AddModelError(kvp.Key, kvp.Value);
        }
    }
}
```

**2. ModelState Exclusions**

For auto-generated/auto-populated fields:
```csharp
ModelState.Remove("Product.Title");   // Auto-copied from Variant.Title
ModelState.Remove("Product.Handle");  // Auto-generated
ModelState.Remove("Variant.Handle");  // Auto-generated
```

---

## Special Patterns

### 1. Review-Before-Save Pattern

**Purpose:** Allow users to review all entered data before committing to database

**Implementation:**

```csharp
// Step 1: Create action validates and stores in TempData
var json = JsonSerializer.Serialize(viewModel);
TempData[ReviewTempDataKey] = json;
return RedirectToAction(nameof(Review));

// Step 2: Review action displays data
var json = TempData[ReviewTempDataKey] as string;
TempData.Keep(ReviewTempDataKey); // Preserve for ConfirmCreate or Back to Edit
var viewModel = JsonSerializer.Deserialize<CreateProductViewModel>(json);
return View(viewModel);

// Step 3: ConfirmCreate saves to database
var json = TempData[ReviewTempDataKey] as string;
// DO NOT call TempData.Keep() - consume the data
var viewModel = JsonSerializer.Deserialize<CreateProductViewModel>(json);
// ... save to database ...
```

**Benefits:**
- Prevents accidental submissions
- Allows correction before database insert
- Supports "Back to Edit" workflow via `fromReview` parameter
- Large data supported via session-based TempData (30-min timeout)

**Note:** TempData is cleared after successful save OR when user visits Create form without `fromReview=true`

---

### 2. Soft Delete Architecture

**Philosophy:** Never permanently delete user-generated data

**Implementation:**
- All entities have `IsActive` bool field
- "Delete" actions set `Status = Archived` and `IsActive = false`
- Default queries filter `WHERE IsActive = true` or `Status != Archived`
- Restore action reverses soft delete

**Benefits:**
- Data recovery without backups
- Audit trail preservation
- Regulatory compliance (GDPR right to erasure handled separately)

**Cascade Behavior:**
- ProductAttribute → ProductVariant (Cascade Delete disabled, relies on soft delete)
- ProductVariant → Product (Restrict: cannot delete product with variants)

---

### 3. Flexible Attribute System

**Design Rationale:**
- Go-kart parts have highly variable specifications
- Traditional fixed columns (Color, Size, Material) don't fit all products
- Need to distinguish customer-selectable options from specifications

**Implementation:**

**IsVariantAttribute Flag:**
- `true` = Customer-facing variant option (e.g., Color: Red vs. Blue)
- `false` = Specification/technical detail (e.g., Material: Chromoly Steel)

**Example:**

Product: Racing Steering Wheel

**Variant Attributes (IsVariantAttribute=true):**
- Color: Black
- Diameter: 10"

**Specifications (IsVariantAttribute=false):**
- Grip Material: Suede
- Bolt Pattern: 6x70mm
- Weight: 450g

**DisplayOrder:**
- Controls rendering sequence on Details page
- Defaults to row index (0, 1, 2, ...)
- User can override for custom sorting

---

### 4. Handle Generation & Uniqueness

**Generation Algorithm:**
```csharp
private string GenerateHandle(string title)
{
    if (string.IsNullOrWhiteSpace(title)) return string.Empty;

    string handle = title.ToLower();
    handle = handle.Replace(" ", "-");
    handle = Regex.Replace(handle, "[^a-z0-9-]", "");
    handle = Regex.Replace(handle, "-+", "-");
    handle = handle.Trim('-');

    return handle;
}
```

**Uniqueness Strategy (ProductsController):**
```csharp
private async Task<string> EnsureUniqueHandle(string baseHandle)
{
    string handle = baseHandle;
    int suffix = 1;

    while (await _context.Products.AnyAsync(p => p.Handle == handle))
    {
        handle = $"{baseHandle}-{suffix}";
        suffix++;
    }

    return handle;
}
```

**Note:** ProductVariantsController does NOT implement auto-suffixing; relies on user changing title to resolve conflicts (detected via DbUpdateException).

---

### 5. AJAX Real-Time Validation

**SKU Uniqueness Check:**

**Client-Side (JavaScript):**
```javascript
var skuTimeout;
document.getElementById('Variant_SKU').addEventListener('blur', function() {
    clearTimeout(skuTimeout);
    skuTimeout = setTimeout(function() {
        var sku = document.getElementById('Variant_SKU').value;
        var variantId = document.getElementById('Variant_VariantID')?.value || 0;

        fetch(`/ProductVariants/CheckSku?sku=${sku}&variantId=${variantId}`)
            .then(res => res.json())
            .then(data => {
                var feedback = document.getElementById('skuFeedback');
                if (!data.isAvailable) {
                    feedback.textContent = 'SKU already in use';
                    feedback.className = 'text-danger';
                } else {
                    feedback.textContent = '';
                }
            });
    }, 500); // Debounce 500ms
});
```

**Server-Side (Controller):**
```csharp
[HttpGet]
public async Task<IActionResult> CheckSku(string sku, int? variantId)
{
    if (string.IsNullOrWhiteSpace(sku))
        return Json(new { isAvailable = true });

    var exists = await _context.ProductVariants
        .AnyAsync(v => v.SKU == sku && v.VariantID != (variantId ?? 0));

    return Json(new { isAvailable = !exists });
}
```

**Benefits:**
- Instant feedback (no form submission required)
- Prevents duplicate SKU errors before save
- Excludes current variant during edit (variantId parameter)

---

### 6. TempData Session Configuration

**Program.cs:**
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

app.UseSession();
```

**Why Session-Based TempData?**
- Default cookie-based TempData limited to ~4KB
- CreateProductViewModel JSON can exceed 4KB with many attributes
- Session storage supports larger payloads
- 30-minute timeout prevents data loss during form filling

---

## Known Issues

### 1. Demo Data Auto-Fill Bug ⚠️ CRITICAL

**Location:** `Views/ProductVariants/Create.cshtml` (Lines 614-750)

**Trigger:** `Ctrl + ` ` (backtick key)

**Problem:**
- Hardcoded SKU values (`BA-4501`, `VX-2200`, `MG-3100`)
- Hardcoded Titles (generate same handles every time)
- Hardcoded Barcodes

**Impact:**
- **First use:** Works fine
- **Second use:** If same product randomly selected → 100% failure (UNIQUE constraint violation)
- **Third use (after 3 products saved):** 100% failure rate

**Error Messages:**
```
"The SKU 'BA-4501' is already in use. Please go back and change it."
"A product with this title already exists. Please go back and change the title."
```

**Root Cause:**
```javascript
const demoProducts = [
    { sku: 'BA-4501', title: 'Birel ART CRY-S12 Racing Chassis Frame', ... },
    { sku: 'VX-2200', title: 'Vortex ROK GP Engine Rebuild Kit', ... },
    { sku: 'MG-3100', title: 'MG SM Yellow Racing Tire Set', ... }
];

// Random selection (1 in 3 chance of duplicate on 2nd use)
const demo = demoProducts[Math.floor(Math.random() * demoProducts.length)];
```

**Recommended Fix:**
```javascript
// Add timestamp-based randomization
const timestamp = Date.now();
const random = Math.floor(Math.random() * 1000);

setInputValue('Variant_SKU', `${demo.sku}-${timestamp}`);
setInputValue('Variant_Title', `${demo.title} (Demo ${random})`);
setInputValue('Variant_Barcode', `${demo.barcode}${random}`);
```

**Severity:** HIGH (breaks demo/testing workflow after first use per product)

**Workaround:** Manually edit SKU/Title after auto-fill before submitting

---

### 2. Inconsistent Handle Uniqueness Handling

**Problem:**
- `ProductsController.EnsureUniqueHandle()` auto-suffixes (`my-product-1`, `my-product-2`)
- `ProductVariantsController` does NOT auto-suffix (throws DbUpdateException)

**Impact:**
- Inconsistent UX between Products and ProductVariants
- ProductVariants require manual title change to resolve conflicts

**Recommended Fix:**
- Implement `EnsureUniqueHandle()` in ProductVariantsController
- OR: Document this as intended behavior (force users to choose unique titles)

**Severity:** MEDIUM (UX inconsistency, but validation prevents data corruption)

---

### 3. Weight Unit Display on Edit

**Problem:**
- Weight stored as integer (grams) in database
- Edit form displays raw grams in input field
- No reverse conversion to user's preferred unit (kg/lb/oz)

**Impact:**
- User sees `28500` instead of `28.5 kg`
- Confusing UX for large weights

**Recommended Fix:**
```csharp
// In Edit action, convert grams to preferred unit
ViewData["WeightValue"] = variant.Weight / 1000m; // kg
ViewData["WeightUnit"] = "kg";
```

**Severity:** LOW (cosmetic UX issue, data integrity unaffected)

---

### 4. Missing Pagination Persistence

**Problem:**
- Pagination state (pageNumber, pageSize) not persisted in filter links
- Clicking filter/search resets to page 1

**Impact:**
- User on page 3 → Applies filter → Jumps to page 1
- Annoying for large datasets

**Recommended Fix:**
- Include `pageNumber` and `pageSize` in all filter/search form submissions
- Preserve current page when possible

**Severity:** LOW (minor UX annoyance)

---

### 5. No Bulk Operations

**Gap:** Cannot bulk publish/unpublish/archive products

**Impact:**
- Must publish/archive one product at a time
- Time-consuming for large catalogs

**Recommended Enhancement:**
- Checkbox selection on Index page
- "Bulk Actions" dropdown (Publish, Unpublish, Archive)

**Severity:** LOW (feature gap, not a bug)

---

## Future Enhancements

### Planned Features

1. **Image Upload System**
   - Product images (primary + gallery)
   - Variant-specific images
   - Thumbnail generation
   - Azure Blob Storage or local wwwroot storage

2. **Import/Export**
   - CSV import for bulk product creation
   - Excel export for reporting
   - Template download for import format

3. **Inventory Management**
   - Stock adjustment history
   - Low stock alerts
   - Automatic reorder points

4. **Pricing Tiers**
   - Dealer pricing vs. Retail pricing
   - Quantity-based discounts
   - Role-based price visibility

5. **Multi-Variant Products**
   - Single parent product with multiple SKU variants (e.g., T-shirt with 6 size/color combinations)
   - Variant attribute matrix (Size × Color)
   - Shared product description, separate variant pricing

6. **Search Improvements**
   - Full-text search (SQLite FTS5)
   - Fuzzy matching for typos
   - Search history/suggestions

7. **API Endpoints**
   - RESTful API for external integrations
   - OAuth 2.0 authentication
   - Swagger/OpenAPI documentation

8. **Activity Log**
   - User action tracking (who edited what, when)
   - Change history (before/after values)
   - Audit report generation

9. **Role-Based Access Control**
   - Admin (full CRUD)
   - Editor (create/edit, no delete)
   - Viewer (read-only)
   - Dealer (public catalog view)

10. **Performance Optimizations**
    - Redis caching for category/vendor dropdowns
    - ETag caching for product images
    - Database query optimization (compiled queries)

---

## Development Guidelines

### Adding a New Entity

1. **Create Model Class** (inherit from `Auditable`)
2. **Add DbSet** to `PontelloDbContext`
3. **Create Migration:** `dotnet ef migrations add MigrationName --context PontelloDbContext --output-dir Data/PIMigrations`
4. **Update Database:** `dotnet ef database update --context PontelloDbContext`
5. **Add Controller** (scaffold or manual)
6. **Create Views** (Index, Create, Edit, Details, Delete)
7. **Update Seed Data** (if needed)
8. **Add Navigation Links** in `_AdminLayout.cshtml`

---

### Database Migrations

**Commands:**
```bash
# Add migration
dotnet ef migrations add MigrationName --context PontelloDbContext --output-dir Data/PIMigrations

# Update database
dotnet ef database update --context PontelloDbContext

# Rollback migration
dotnet ef database update PreviousMigrationName --context PontelloDbContext

# Remove last migration (if not applied)
dotnet ef migrations remove --context PontelloDbContext

# Generate SQL script
dotnet ef migrations script --context PontelloDbContext --output migration.sql
```

---

### Testing Workflow

1. **Reset Database:**
   ```bash
   rm PontelloImportDatabase.db
   dotnet ef database update --context ApplicationDbContext
   dotnet ef database update --context PontelloDbContext
   dotnet run
   ```

2. **Seed Data Verification:**
   - Check that 9 vendors, 6 categories, 8 products, 20+ variants exist
   - Verify attributes are correctly linked

3. **Test Create Flow:**
   - Fill form → Submit → Review → Confirm → Verify in Details page
   - Test "Back to Edit" → Modify → Re-submit

4. **Test Validation:**
   - Submit empty form (check required field errors)
   - Duplicate SKU (check uniqueness error)
   - Invalid SKU format (check regex error)

5. **Test Soft Delete:**
   - Archive variant → Verify in Archived tab
   - Restore variant → Verify in Draft tab

---

## Deployment Checklist

### Pre-Deployment

- [ ] Update `appsettings.Production.json` with production connection string
- [ ] Change `ASPNETCORE_ENVIRONMENT` to `Production`
- [ ] Enable HTTPS enforcement
- [ ] Configure email SMTP settings (for Identity email confirmation)
- [ ] Set strong JWT secrets (if implementing API)
- [ ] Review security headers (HSTS, X-Frame-Options, CSP)
- [ ] Minify CSS/JS assets
- [ ] Enable response compression
- [ ] Configure logging (Application Insights, Serilog, etc.)
- [ ] Backup database before migration

### Post-Deployment

- [ ] Run database migrations
- [ ] Verify seed data loaded correctly
- [ ] Test authentication flow (registration, login, logout)
- [ ] Smoke test all CRUD operations
- [ ] Check error pages (404, 500)
- [ ] Verify email sending (confirmation, password reset)
- [ ] Monitor performance (response times, database queries)
- [ ] Set up automated backups

---

## Troubleshooting

### "UNIQUE constraint failed: ProductVariants.SKU"

**Cause:** Attempting to create variant with duplicate SKU

**Solution:**
- Check existing SKUs in database
- Use AJAX validation to catch duplicates before submission
- Consider auto-generating SKUs (prefix + auto-increment)

---

### "Product data was lost. Please fill out the form again."

**Cause:** TempData expired (session timeout or browser closed)

**Solution:**
- Increase session timeout in `Program.cs` (currently 30 minutes)
- Warn users before session expires (JavaScript countdown)
- Consider saving draft data to database instead of TempData

---

### "Cannot delete product because it has X variant(s)."

**Cause:** Attempting to delete parent Product with related ProductVariants

**Solution:**
- Archive all variants first
- OR: Implement cascade soft delete (archive product → archive all variants)

---

### Demo Auto-Fill Not Working

**Symptoms:** Pressing `Ctrl + ` ` does nothing

**Possible Causes:**
1. Focus not on page (click inside browser first)
2. Browser intercepts shortcut (try different browser)
3. JavaScript error (check Console)

**Solution:**
- Open Browser DevTools → Console tab
- Look for JavaScript errors
- Try refreshing page (Ctrl+F5)

---

## Contact & Support

**Project Owner:** Pontello Motorsports IT Team
**Framework:** ASP.NET Core 9.0
**Database:** SQLite (local), PostgreSQL (production recommended)
**License:** Proprietary

---

**End of Documentation**

*Generated: February 2026*
*Total Implementation: ~1,500 lines of code (Controllers, Models, DbContext)*
*Total Database Entities: 5 core + 2 Identity*
*Total Seeded Records: 9 vendors, 6 categories, 8 products, 20+ variants, 30+ attributes*
