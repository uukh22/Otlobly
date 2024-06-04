# Otlobly

Otlobly is a food ordering website developed using ASP.NET Core MVC 7. It is designed to streamline the food ordering process with distinct user roles and functionalities.

## User Roles and Responsibilities

### Superadmin
- Appoints Admins.
- Demotes Admins to Customers.
- Locks user accounts.
- Manages categories, subcategories, items, and coupons (add, edit, delete).
- Updates order statuses.

### Admin
- Cannot modify Superadmin permissions or lock Superadmin accounts.
- Locks user accounts.
- Manages categories, subcategories, items, and coupons (add, edit, delete).
- Updates order statuses.

### Customer
- Places orders.
- Tracks order statuses.
- Makes payments.
- Cancels orders.

## Technologies and Tools Used
- LINQ
- Class Library
- Areas
- Identity
- Session
- MVVM (Model-View-ViewModel)
- View Components
- View Model
- Stripe API

---

This readme provides a high-level overview of Otlobly, its user roles, and the technologies used in its development. For more detailed information, please refer to the project documentation or contact the project maintainers.