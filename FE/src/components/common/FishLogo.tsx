import React from 'react';

interface FishLogoProps {
  size?: number;
  className?: string;
}

const FishLogo: React.FC<FishLogoProps> = ({ size = 40, className }) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 64 64"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    className={className}
    aria-hidden="true"
  >
    <defs>
      <linearGradient id="fish-body" x1="14" y1="14" x2="50" y2="50" gradientUnits="userSpaceOnUse">
        <stop stopColor="#7C5CFF" />
        <stop offset="0.55" stopColor="#4C8DF6" />
        <stop offset="1" stopColor="#35D6C9" />
      </linearGradient>
      <linearGradient id="fish-fin" x1="38" y1="22" x2="58" y2="34" gradientUnits="userSpaceOnUse">
        <stop stopColor="#FFD166" />
        <stop offset="1" stopColor="#FF9F1C" />
      </linearGradient>
    </defs>

    <path
      d="M15 31C15 21.5 23.4 14 34.8 14C40.6 14 46 16 50 19.5L58 16L56 25C57.3 27 58 29 58 31C58 33 57.3 35 56 37L58 46L50 42.5C46 46 40.6 48 34.8 48C23.4 48 15 40.5 15 31Z"
      fill="url(#fish-body)"
    />
    <path d="M18 31L9 23V39L18 31Z" fill="#5A3FC0" />
    <path
      d="M38 22C41.5 24.2 43.5 27.4 43.5 31C43.5 34.6 41.5 37.8 38 40V22Z"
      fill="#AEEBFF"
      fillOpacity="0.72"
    />
    <path
      d="M46 21L58 25L52.5 31L58 37L46 41C47.4 37.9 48.2 34.5 48.2 31C48.2 27.5 47.4 24.1 46 21Z"
      fill="url(#fish-fin)"
    />
    <circle cx="27" cy="27" r="2.5" fill="#102A43" />
    <circle cx="26.2" cy="26.2" r="0.8" fill="#FFFFFF" />
    <path
      d="M24.5 35C27.5 37.5 32.5 37.7 35.8 35.5"
      stroke="#F5FBFF"
      strokeWidth="2.2"
      strokeLinecap="round"
    />
    <circle cx="13" cy="26" r="1.2" fill="#C9B8FF" fillOpacity="0.9" />
    <circle cx="10" cy="31" r="0.9" fill="#C9B8FF" fillOpacity="0.7" />
    <circle cx="13" cy="36" r="1.1" fill="#C9B8FF" fillOpacity="0.8" />
  </svg>
);

export default FishLogo;
