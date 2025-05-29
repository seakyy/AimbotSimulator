# AimbotSimulator

This project demonstrates UI logic, real-time event handling, and low-level pointer manipulation in a modern WPF desktop application.

It was built as a **hands-on simulation tool** to showcase:
- Dynamic user interfaces in WPF
- Smooth user interaction
- Real-time feedback with zero memory allocation beyond core stack use
- Memory-efficient pointer control using C#'s `unsafe` mode

---

## Controls
```text
F1     Toggle Aimbot  
F2     Toggle between Smooth Aim and Insta-Lock 
F3     Toggle Triggerbot  
Esc    Exit
```

---

## Tech Highlights

- **WPF (MV-style layout)** – Real-time interface with Canvas rendering and control overlay
- **Unsafe C#** – Direct cursor manipulation using `SetCursorPos` with stack-allocated memory
- **DispatcherTimer** – High-frequency event dispatching for frame-perfect updates
- **Custom Control Panel** – Toggle UI features (aimbot, smoothing, rage mode) and tune behavior live
- **Rage Mode** – Automated targeting with execution time benchmarking
- **No external libraries** – 100% native .NET and Windows API

---

# Showcase

![AimbotSimulator](https://github.com/user-attachments/assets/719572e0-58d7-43f9-8a1b-7428f03a227c)

---


# Overview Diagram
![diagram](https://github.com/user-attachments/assets/19ec6b4e-f0a0-4c53-a172-f5fe2cc9902a)

---

## Requirements

- .NET 6 or later  
- Visual Studio (Enable "Allow unsafe code" in project settings)

---

##  Important Disclaimer!

This simulator is ONLY for demonstration and learning purposes. It does not interact with or modify any external applications or games!

