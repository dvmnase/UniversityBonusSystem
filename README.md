# University Bonus System

A comprehensive C# console application for mass bonus awards in university environment with idempotent operations and advanced reporting. Developed as part of university coursework demonstrating OOP principles, LINQ, XML processing, and advanced C# features.

##  Features

- ** Mass Bonus Awards** - Process purchase data from XML files with automatic bonus calculation
- ** Idempotent Operations** - Prevent duplicate processing using SHA256 hash keys
- ** Advanced Reporting** - Generate TXT, CSV, and XML reports with LINQ analytics
- ** LINQ Integration** - Both LINQ to XML and LINQ to Objects for complex data processing
- ** Operation Logging** - Comprehensive success/error tracking with file logging
- ** Interactive Menu** - User-friendly console interface with 9 functional options
- ** OOP Architecture** - Full object-oriented design with inheritance and polymorphism

##  Architecture & Design Patterns

### Object-Oriented Programming
- **Base Class with Virtual Methods** - `Department` class with overridable `CalculateTotalBonus()` method
- **Partial Classes** - `Student` class split across `Student.cs` and `Student.Methods.cs`
- **Inheritance Hierarchy** - Proper class relationships between departments, courses, and students

### C# Advanced Features
- **Extension Methods** - Custom string operations in `StringExtensions.cs`:
  - `ToSha256Hash()` - Generate idempotency keys
  - `IsValidCardNumber()` - Validate card number format
  - `Truncate()` - String truncation for display
- **Delegates & Events** - `BonusAwardedEventHandler` for real-time notification system
- **Exception Handling** - Comprehensive error handling with file logging

### Data Processing
- **LINQ to XML** - Read, validate, and modify XML documents with full validation
- **LINQ to Objects** - Complex queries with grouping, aggregation, projections
- **Collection Operations** - Efficient data manipulation with generics

## 🚀 Quick Start

### Prerequisites
- .NET 6.0 SDK or later
- Git

