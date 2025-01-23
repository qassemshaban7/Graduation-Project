# Graduation Project - School Management Platform

## Overview
Awarded **"Excellent"**, this graduation project is a comprehensive web-based **School Management System** integrated with a RESTful API. The platform effectively manages school operations, leveraging technology to streamline processes for **administrators**, **teachers**, and **parents**.

### Key Features:
- **Web Application and RESTful API:** A unified backend serves both a user-friendly website and a robust API for external integrations.
- **Automated Student Promotion and Attendance Tracking:** Utilizing Hangfire to manage term-based and year-based student promotions, and ensuring attendance is checked twice daily, excluding holidays.
- **Advanced Analytics and Visualizations:** Interactive charts for survey results, grades, and behavior, providing insights for parents and administrators.
- **AI and IoT Integration:** Collaboration with AI and embedded teams to enhance attendance tracking through cameras and automated data reporting.

---

## Project Roles

### 1. Administrator
Administrators have full control over the platform, including:
- Managing students, teachers, parents, years, terms, and subjects.
- Defining surveys for parents to fill out.
- Reviewing survey results visualized as detailed charts.
- Ensuring smooth transitions for students between terms and years.

### 2. Teacher
Teachers play a vital role by:
- Adding exams, quizzes, tasks, and behavioral grades for students.
- Recording and updating students' scores.
- Monitoring classroom activities and providing data to parents via reports.

### 3. Parent
Parents benefit from:
- Accessing their child's attendance records and downloading reports in Excel format.
- Monitoring behavior and academic performance with visualized data (charts and reports).
- Participating in surveys to provide feedback to the school.

---

## Core Features

### Web Application
- **Role-based Access:** Each user type has tailored access to their specific features.
- **Interactive Charts:**
  - Administrators: View survey results and academic trends.
  - Parents: Analyze their child’s grades and behavior.
- **Attendance Tracking:** Comprehensive reports and downloadable Excel files. Attendance is checked twice daily, with holidays excluded.
- **Academic Records:** Detailed records for each student’s grades, including subject-wise and term-wise breakdowns.

### RESTful API
- **Full Feature Parity with the Website:** A complete API implementation that mirrors the functionality of the website, providing a seamless RESTful interface.
- **Integration with Cameras:** AI-powered attendance tracking by:
  - Sending student names and images to cameras for identification.
  - Receiving attendance data for real-time updates.
- **Extensible Design:** Can be integrated with mobile or other external systems.

### Automated Student Management
- **Hangfire Integration:**
  - Promotes students to the next term or year after passing exams and completing term durations.
  - Automatically marks students as graduates after completing all academic years.
  - Tracks attendance after each period to ensure compliance with school schedules.
  
---

## Technologies Used

- **Backend:** ASP.NET Core with Entity Framework
- **Frontend:** HTML, CSS, Bootstrap, JavaScript
- **Database:** SQL Server
- **Task Scheduling:** Hangfire
- **AI Integration:** Collaborated with AI and embedded teams for attendance management.
- **Visualization:** Chart.js for interactive data visualizations

---

## Project Links
- **Website:** [Live Website](https://ablexav1web.runasp.net)
- **API:** [API Documentation](https://ablexav1.runasp.net/swagger/index.html)

---

## Contact

For any questions or contributions, feel free to contact:
- **GitHub:** [qassemshaban7](https://github.com/qassemshaban7)
- **LinkedIn:** [LinkedIn Profile](https://www.linkedin.com/in/qassemshaban)
- **Email:** [qassemshaban7@gmail.com]
