import React, { useEffect, useState } from 'react';
import { Search, Plus, HardDrive } from 'lucide-react';

interface HeaderProps {
  title: string;
  subtitle?: string;
  onSearchClick?: () => void;
  onNewPatientClick?: () => void;
  searchValue?: string;
  onSearchChange?: (val: string) => void;
}

export const Header: React.FC<HeaderProps> = ({
  title,
  subtitle,
  onSearchClick,
  onNewPatientClick,
  searchValue = '',
  onSearchChange,
}) => {
  const [isMac, setIsMac] = useState(false);

  useEffect(() => {
    setIsMac(navigator.platform.toUpperCase().indexOf('MAC') >= 0);

    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        onSearchClick?.();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [onSearchClick]);

  return (
    <header className="h-16 bg-white border-b border-botanical-stone px-6 flex items-center justify-between shadow-sm z-10">
      {/* Title & View Context */}
      <div>
        <h2 className="text-lg font-bold text-botanical-graphite leading-none flex items-center">
          {title}
        </h2>
        {subtitle && (
          <p className="text-xs text-botanical-muted mt-1 leading-none">{subtitle}</p>
        )}
      </div>

      {/* Global Search Bar (Omnibox Ctrl+K) */}
      <div className="flex-1 max-w-md mx-6">
        <div className="relative">
          <Search className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" />
          <input
            type="text"
            value={searchValue}
            onChange={(e) => onSearchChange?.(e.target.value)}
            placeholder="Buscar por mascota, acudiente, documento o celular..."
            className="w-full pl-9 pr-20 py-2 text-sm bg-botanical-linen/80 border border-botanical-stone rounded-xl text-botanical-graphite placeholder-botanical-subtle focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald transition-all"
          />
          <div className="absolute right-2.5 top-1/2 -translate-y-1/2 pointer-events-none">
            <kbd className="px-1.5 py-0.5 text-[10px] font-semibold text-botanical-subtle bg-white border border-botanical-stone rounded shadow-sm">
              {isMac ? '⌘K' : 'Ctrl+K'}
            </kbd>
          </div>
        </div>
      </div>

      {/* Actions & Offline Station Pill */}
      <div className="flex items-center space-x-3">
        {/* Local Station Indicator */}
        <div className="hidden lg:flex items-center space-x-1.5 px-3 py-1 bg-emerald-50 border border-emerald-200 text-emerald-800 rounded-lg text-xs font-medium">
          <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
          <HardDrive className="w-3.5 h-3.5 text-emerald-600 inline" />
          <span>Localhost:5000</span>
        </div>

        {onNewPatientClick && (
          <button
            onClick={onNewPatientClick}
            className="flex items-center px-3.5 py-2 rounded-xl bg-botanical-emerald hover:bg-botanical-emeraldHover text-white text-xs font-semibold shadow-sm transition-all duration-150 transform active:scale-95"
          >
            <Plus className="w-4 h-4 mr-1.5" />
            <span>Nuevo Registro</span>
          </button>
        )}
      </div>
    </header>
  );
};
