let timerInterval;
let totalSeconds = 0;
let originalTotalSeconds = 0;
let isRunning = false;
let isPaused = false;
let currentSubject = '';
let sessionStartTime = null;
let isFloatingMinimized = false;

// DOM elements (some may not exist on every page)
const studyForm = document.getElementById('studyForm');
const timerSection = document.getElementById('timerSection');
const startStudyBtn = document.getElementById('startStudyBtn');
const timerDisplay = document.getElementById('timerDisplay');
const studySubject = document.getElementById('studySubject');
const sessionDuration = document.getElementById('sessionDuration');
const progressBar = document.getElementById('progressBar');
const pauseBtn = document.getElementById('pauseBtn');
const resumeBtn = document.getElementById('resumeBtn');
const stopBtn = document.getElementById('stopBtn');

// Floating timer
const floatingTimer = document.getElementById('floatingTimer');
const floatingDisplay = document.getElementById('floatingDisplay');
const floatingSubject = document.getElementById('floatingSubject');
const floatingPauseBtn = document.getElementById('floatingPauseBtn');
const floatingResumeBtn = document.getElementById('floatingResumeBtn');
const floatingStopBtn = document.getElementById('floatingStopBtn');

// Modal
const sessionCompleteModal = document.getElementById('sessionCompleteModal');
const completedSubject = document.getElementById('completedSubject');
const completedDuration = document.getElementById('completedDuration');

// Load state on page load
window.addEventListener('load', function () {
if (typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn) {
    loadTimerState();
}
});

// Save state
function saveTimerState() {
    const timerState = {
        totalSeconds,
        originalTotalSeconds,
        isRunning,
        isPaused,
        currentSubject,
        timestamp: Date.now(),
        sessionStartTime: sessionStartTime ? sessionStartTime.toISOString() : null,
        sessionCompleted: totalSeconds <= 0 
    };
    localStorage.setItem('timerState', JSON.stringify(timerState));
}

// Load state
function loadTimerState() {
    const saved = localStorage.getItem('timerState');
    const alertShownKey = 'pausedSessionAlertShown';

    if (!saved) {
        // No saved timer state at all
        localStorage.removeItem(alertShownKey);
        return;
    }

    const state = JSON.parse(saved);
    const timePassed = Math.floor((Date.now() - state.timestamp) / 1000);

    // Calculate remaining time
    const remainingSeconds = Math.max(0, state.totalSeconds - (state.isRunning ? timePassed : 0));

    totalSeconds = remainingSeconds;
    originalTotalSeconds = state.originalTotalSeconds;
    currentSubject = state.currentSubject;
    sessionStartTime = state.sessionStartTime ? new Date(state.sessionStartTime) : new Date();
    if (localStorage.getItem('loggedOutDuringSession') === 'true') {
    isRunning = false;
    isPaused = true;
    localStorage.removeItem('loggedOutDuringSession');
}


    // Restore the running/paused state
    //isRunning = state.isRunning && remainingSeconds > 0;
    //isPaused = state.isPaused || (!state.isRunning && remainingSeconds > 0);
    if (state.isRunning && remainingSeconds > 0) {
        isRunning = true;
        isPaused = false;
    } else if (state.isPaused && remainingSeconds > 0) {
        isRunning = false;
        isPaused = true;
    } else {
        isRunning = false;
        isPaused = false;
    }

    if (state.sessionCompleted || totalSeconds <= 0) {
        localStorage.removeItem('timerState');
        localStorage.removeItem(alertShownKey);

        // ✅ Strictly prevent showing modal more than once
        if (!localStorage.getItem('sessionCompleteShown')) {
            showSessionComplete();
            localStorage.setItem('sessionCompleteShown', 'true');
        } else {
            // Hide the modal just in case the layout had it visible already
            if (sessionCompleteModal) sessionCompleteModal.classList.remove('show');
        }
        return;
    }




    // Update UI elements...
    if (studySubject) studySubject.textContent = currentSubject;
    if (floatingSubject) floatingSubject.textContent = currentSubject;
    if (sessionDuration) sessionDuration.textContent = formatDuration(originalTotalSeconds);
    if (studyForm) studyForm.style.display = 'none';
    if (timerSection) timerSection.classList.add('show');
    showFloatingTimer();
    updateDisplay();
    updateFloatingControls();

    // Resume timer if it was running
    if (isRunning) {
        startTimer();
    }

    // Show alert only if user is logged in, timer is paused, and alert not shown yet
    if (isPaused && !localStorage.getItem(alertShownKey) && typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn) {
        Swal.fire({
            title: `Welcome back!`,
            html: `You still have <b>${formatTime(totalSeconds)}</b> left on <b>${currentSubject}</b>.<br><br>Click "Resume" when you're ready.`,
            icon: 'info',
            confirmButtonText: 'Got it!',
            confirmButtonColor: '#3085d6'
        });
        localStorage.setItem(alertShownKey, 'true');
    }
}
const logoutBtn = document.getElementById('logoutBtn');

const logoutForm = document.getElementById('logoutForm');

if (logoutForm) {
    logoutForm.addEventListener('submit', function (e) {
        e.preventDefault(); // stop the form from submitting immediately

        if (isRunning) {
            pauseTimer();
            localStorage.setItem('loggedOutDuringSession', 'true');
        }

        // Wait briefly for timer to pause and then submit form
        setTimeout(() => {
            logoutForm.submit(); // now submit the form properly
        }, 300); // delay just long enough to ensure pauseTimer() completes
    });
}


        
    



// Helpers
function formatTime(seconds) {
    const h = String(Math.floor(seconds / 3600)).padStart(2, '0');
    const m = String(Math.floor((seconds % 3600) / 60)).padStart(2, '0');
    const s = String(seconds % 60).padStart(2, '0');
    return `${h}:${m}:${s}`;
}

function formatDuration(seconds) {
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;

    if (h > 0) return `${h}h ${m}m`;
    if (m > 0) return `${m}m ${s}s`;
    return `${s}s`;
}

async function finishNow() {

    console.log("🔥 finishNow() running...");
    // Save the session to database
    await saveSessionToDatabase();

    // Hide modal
    if (sessionCompleteModal) {
        sessionCompleteModal.classList.remove('show');
        sessionCompleteModal.style.display = 'none';
    }

    // Clear timer
    clearInterval(timerInterval);
    isRunning = false;
    isPaused = false;

    // Reset everything
    totalSeconds = 0;
    originalTotalSeconds = 0;
    currentSubject = '';
    sessionStartTime = null;

    // Clear storage
    localStorage.removeItem('timerState');
    localStorage.removeItem('sessionCompleteShown');
    localStorage.removeItem('pausedSessionAlertShown');

    // Reset UI completely
    if (timerDisplay) timerDisplay.classList.remove('finished');
    if (progressBar) progressBar.style.width = '0%';
    if (studyForm) studyForm.style.display = 'block';
    if (timerSection) timerSection.classList.remove('show');
    hideFloatingTimer();

    if (pauseBtn) pauseBtn.style.display = 'inline-block';
    if (resumeBtn) resumeBtn.style.display = 'none';
    if (floatingPauseBtn) floatingPauseBtn.style.display = 'inline-block';
    if (floatingResumeBtn) floatingResumeBtn.style.display = 'none';
}

function updateDisplay() {
    const timeStr = formatTime(totalSeconds);
    if (timerDisplay) timerDisplay.textContent = timeStr;
    if (floatingDisplay) floatingDisplay.textContent = timeStr;

    const progress = ((originalTotalSeconds - totalSeconds) / originalTotalSeconds) * 100;
    if (progressBar) progressBar.style.width = progress + '%';

    saveTimerState();
}

// Timer controls
function startTimer() {
    if (!sessionStartTime) {
        sessionStartTime = new Date();
    }

    isRunning = true;
    isPaused = false;

    timerInterval = setInterval(() => {
        if (totalSeconds > 0) {
            totalSeconds--;
            updateDisplay();
        } else {
            // ✅ When timer naturally ends, just show modal - NO SAVING
            showSessionComplete();
        }
    }, 1000);
}



function pauseTimer() {
    if (isRunning) {
        clearInterval(timerInterval);
        isRunning = false;
        isPaused = true;
        saveTimerState();
        console.log("Paused");
        updateFloatingControls(); // move here
    }
}


function resumeTimer() {
    if (isPaused && totalSeconds > 0) {
        startTimer(); 
    }
}

function updateFloatingControls() {
    // Only update floating buttons if they exist
    if (floatingPauseBtn && floatingResumeBtn) {
        if (isRunning) {
            floatingPauseBtn.style.display = 'inline-block';
            floatingResumeBtn.style.display = 'none';
        } else if (isPaused) {
            floatingPauseBtn.style.display = 'none';
            floatingResumeBtn.style.display = 'inline-block';
        }
    }

    // Also handle regular buttons
    if (pauseBtn && resumeBtn) {
        if (isRunning) {
            pauseBtn.style.display = 'inline-block';
            resumeBtn.style.display = 'none';
        } else if (isPaused) {
            pauseBtn.style.display = 'none';
            resumeBtn.style.display = 'inline-block';
        }
    }
    console.log("isRunning:", isRunning, "isPaused:", isPaused);
}


function showFloatingTimer() {
    if (floatingTimer) floatingTimer.classList.add('show');
}

function hideFloatingTimer() {
    if (floatingTimer) floatingTimer.classList.remove('show');
}

function toggleFloatingTimer() {
    isFloatingMinimized = !isFloatingMinimized;
    const controls = document.getElementById('floatingControls');
    if (!controls) return;

    if (isFloatingMinimized) {
        controls.style.display = 'none';
        floatingTimer.classList.add('minimized');
        floatingTimer.classList.remove('expanded');
    } else {
        controls.style.display = 'flex';
        floatingTimer.classList.remove('minimized');
        floatingTimer.classList.add('expanded');
    }
}

function returnToTimer() {
    if (studyForm) studyForm.style.display = 'block';
    if (timerSection) timerSection.classList.add('show');
}

function navigateToPage(page) {
    saveTimerState();
    switch (page) {
        case 'flashcards':
            window.location.href = '/Flashcards/Index';
            break;
        case 'notes':
            window.location.href = '/Notes/Index';
            break;
        default: Swal.fire({
            icon: 'warning',
            title: 'Oops!',
            text: 'Unknown page!',
            confirmButtonText: 'Okay',
            confirmButtonColor: '#3085d6'
        });

    }
}
window.addEventListener('beforeunload', () => {
    if (isRunning || isPaused) {
        saveTimerState();
    }
});

function showSessionComplete() {
    clearInterval(timerInterval);
    isRunning = false;

    const sessionEndTime = new Date();
    const actualDurationInSeconds = sessionStartTime
        ? Math.floor((sessionEndTime - sessionStartTime) / 1000)
        : 0;

    if (completedSubject) completedSubject.textContent = currentSubject || "this subject";
    if (completedDuration) completedDuration.textContent = formatDuration(actualDurationInSeconds);

    if (sessionCompleteModal) {
        sessionCompleteModal.style.display = 'block';
        sessionCompleteModal.classList.add('show');
    }

    localStorage.setItem('sessionCompleteShown', 'true');
}

function extendSession(minutes) {
    const extraSeconds = minutes * 60;
    totalSeconds += extraSeconds;
    originalTotalSeconds += extraSeconds;

    // Hide the modal
    if (sessionCompleteModal) {
        sessionCompleteModal.classList.remove('show');
        sessionCompleteModal.style.display = 'none';
    }

    // Reset the flag so modal can show again when extended time ends
    localStorage.removeItem('sessionCompleteShown');

    updateDisplay();
    startTimer(); // Resume the timer with extended time

    console.log(`Session extended by ${minutes} minutes`);
}
// Add this function to save the session to your database
async function saveSessionToDatabase() {
    console.log("🔥 saveSessionToDatabase called");

    if (!sessionStartTime || !currentSubject) {
        console.log("No session data to save");
        return;
    }

    const sessionEndTime = new Date();
    const actualDurationInSeconds = Math.floor((sessionEndTime - sessionStartTime) / 1000);

    // Create the session object that matches your StudySession model
    const sessionData = {
        Subject: currentSubject,
        StartTime: sessionStartTime.toISOString(),
        EndTime: sessionEndTime.toISOString(),
        Duration: `${Math.floor(actualDurationInSeconds / 3600)}:${Math.floor((actualDurationInSeconds % 3600) / 60)}:${actualDurationInSeconds % 60}`, // Format as TimeSpan string
        // Note: UserId and Id will be set by your controller
    };

    try {
        const response = await fetch('/api/study/save-session', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(sessionData)
        });


        if (response.ok) {
            console.log("Session saved successfully");
            const result = await response.text();
            console.log(result);
        } else {
            console.error("Failed to save session:", response.status, response.statusText);
        }
    } catch (error) {
        console.error("Error saving session:", error);
    }
}

// Call this function when the session ends
// You should add this call to your endSession() function and showSessionComplete() function
function endSession() {
    localStorage.removeItem('pausedSessionAlertShown');
    clearInterval(timerInterval);
    isRunning = false;
    isPaused = false;

    const sessionEndTime = new Date();
    const actualDurationInSeconds = sessionStartTime
        ? Math.floor((sessionEndTime - sessionStartTime) / 1000)
        : 0;

    if (completedSubject) completedSubject.textContent = currentSubject || "this subject";
    if (completedDuration) completedDuration.textContent = formatDuration(actualDurationInSeconds);

    if (sessionCompleteModal) {
        sessionCompleteModal.style.display = 'block';
        sessionCompleteModal.classList.add('show');
    }

    localStorage.setItem('sessionCompleteShown', 'true');
}



document.addEventListener('DOMContentLoaded', function () {
    const startStudyBtn = document.getElementById('startStudyBtn');
    const hoursInput = document.getElementById('hours');
    const minutesInput = document.getElementById('minutes');
    const secondsInput = document.getElementById('seconds');
    const subjectInput = document.getElementById('subject');
    const durationBtns = document.querySelectorAll('.duration-btn');
    //loadTimerState();
    if (!startStudyBtn) {
        console.warn("Start button not found!");

        return;
    }
    durationBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            durationBtns.forEach(b => b.classList.remove('selected'));
            this.classList.add('selected');

            const minutes = parseInt(this.dataset.minutes);
            hoursInput.value = Math.floor(minutes / 60);
            minutesInput.value = minutes % 60;
            secondsInput.value = 0;
        });
    });

 
    startStudyBtn.addEventListener('click', function () {
        const subject = subjectInput.value;
        const hours = parseInt(hoursInput.value) || 0;
        const minutes = parseInt(minutesInput.value) || 0;
        const seconds = parseInt(secondsInput.value) || 0;
        if (!subject.trim()) {
            Swal.fire({
                icon: 'warning',
                title: 'Oops!',
                text: 'Please enter a subject',
                confirmButtonText: 'Okay',
                confirmButtonColor: '#3085d6'
            });
            return;
        }


        totalSeconds = (hours * 3600) + (minutes * 60) + seconds;
        originalTotalSeconds = totalSeconds;
        currentSubject = subject;

        //if (totalSeconds <= 0) return alert('Please set a study duration');
        if (totalSeconds<=0) {
            Swal.fire({
                icon: 'warning',
                title: 'Oops!',
                text: 'Please set a study duration',
                confirmButtonText: 'Okay',
                confirmButtonColor: '#3085d6'
            });
            return;
        }


        if (studySubject) studySubject.textContent = subject;
        if (floatingSubject) floatingSubject.textContent = subject;
        if (sessionDuration) sessionDuration.textContent = formatDuration(totalSeconds);
        updateDisplay();

        if (studyForm) studyForm.style.display = 'none';
        if (timerSection) timerSection.classList.add('show');
        showFloatingTimer();

        sessionStartTime = new Date(); 
        // localStorage.removeItem('sessionCompleteShown'); // Allow modal to show again for new session

        startTimer();
    });
});

if (pauseBtn) pauseBtn.addEventListener('click', pauseTimer);
if (resumeBtn) resumeBtn.addEventListener('click', resumeTimer);
if (stopBtn) {
    stopBtn.addEventListener('click', () => {
        Swal.fire({
            title: 'End session?',
            text: "Are you sure you want to stop this session?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, end it',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                endSession(); // ❌ ONLY call endSession() - no saving here
            }
        });
    });
}

if (floatingStopBtn) {
    floatingStopBtn.addEventListener('click', () => {
        Swal.fire({
            title: 'End session?',
            text: "Are you sure you want to stop this session?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, end it',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                endSession(); 
            }
        });
    });
}

document.addEventListener('visibilitychange', () => {
    if (document.hidden) saveTimerState();
});


document.addEventListener('DOMContentLoaded', function () {
    const finishNowBtn = document.getElementById('finishNowBtn');

    if (finishNowBtn) {
        finishNowBtn.addEventListener('click', async () => {
            console.log("✅ Finish Now clicked");
            await finishNow();  // This is your async function
        });
    } else {
        console.warn("❌ Button with id 'finishNowBtn' not found");
    }
});
