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

## Demo
Check out the live demo:
[LinkedIn Post with Demo](https://www.linkedin.com/posts/uukh2_%D8%A7%D9%84%D8%B3%D9%84%D8%A7%D9%85-%D8%B9%D9%84%D9%8A%D9%83%D9%85-%D9%88%D8%B1%D8%AD%D9%85%D9%87-%D8%A7%D9%84%D9%84%D9%87-%D9%88-%D8%A8%D8%B1%D9%83%D8%A7%D8%AA%D9%87-i-ugcPost-7203581868510711808-b1rR?utm_source=combined_share_message&utm_medium=member_ios)

---

This readme provides a high-level overview of Otlobly, its user roles, and the technologies used in its development. For more detailed information, please refer to the project documentation or contact the project maintainers.
